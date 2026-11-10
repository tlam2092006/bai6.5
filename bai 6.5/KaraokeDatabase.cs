using System.Data;
using Microsoft.Data.SqlClient;

namespace bai_6._5;

public sealed record Room(int Id, string Name, string Type, decimal HourlyRate, string Status);

public sealed record ServiceItem(int Id, string Name, decimal UnitPrice, int Stock);

public sealed record ActiveInvoice(int Id, DateTime StartTime);

public sealed record CheckoutResult(decimal RoomTotal, decimal ServiceTotal, decimal GrandTotal, TimeSpan Duration);

public sealed class KaraokeDatabase
{
    public const string ConnectionString =
        "Server=localhost;Database=KaraokeDB;Integrated Security=True;TrustServerCertificate=True;";

    public List<Room> GetRooms()
    {
        const string sql = "SELECT MaPhong, TenPhong, LoaiPhong, GiaTheoGio, TrangThai FROM PhongHat ORDER BY MaPhong;";
        var rooms = new List<Room>();

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rooms.Add(new Room(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDecimal(3),
                reader.GetString(4)));
        }

        return rooms;
    }

    public List<ServiceItem> GetServices()
    {
        const string sql = "SELECT MaDV, TenDV, DonGia, TonKho FROM DichVu ORDER BY TenDV;";
        var services = new List<ServiceItem>();

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            services.Add(new ServiceItem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetInt32(3)));
        }

        return services;
    }

    public ActiveInvoice? GetActiveInvoice(int roomId)
    {
        const string sql = """
            SELECT TOP (1) MaHD, GioVao
            FROM HoaDon
            WHERE MaPhong = @MaPhong AND TrangThai = 0
            ORDER BY MaHD DESC;
            """;

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MaPhong", roomId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? new ActiveInvoice(reader.GetInt32(0), reader.GetDateTime(1)) : null;
    }

    public DataTable GetOrderedServices(int roomId)
    {
        const string sql = """
            SELECT c.MaDV, d.TenDV, c.SoLuong, c.DonGia,
                   CAST(c.SoLuong * c.DonGia AS decimal(18,2)) AS ThanhTien
            FROM ChiTietDichVu c
            INNER JOIN DichVu d ON d.MaDV = c.MaDV
            INNER JOIN HoaDon h ON h.MaHD = c.MaHD
            WHERE h.MaPhong = @MaPhong AND h.TrangThai = 0
            ORDER BY d.TenDV;
            """;

        using var connection = new SqlConnection(ConnectionString);
        using var adapter = new SqlDataAdapter(sql, connection);
        adapter.SelectCommand!.Parameters.AddWithValue("@MaPhong", roomId);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public void OpenRoom(int roomId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var check = new SqlCommand(
                "SELECT TrangThai FROM PhongHat WITH (UPDLOCK) WHERE MaPhong = @MaPhong;",
                connection, transaction);
            check.Parameters.AddWithValue("@MaPhong", roomId);
            var status = check.ExecuteScalar() as string;
            if (status is null)
                throw new InvalidOperationException("Không tìm thấy phòng.");
            if (status != "Trống")
                throw new InvalidOperationException("Phòng này đang được sử dụng.");

            using var insert = new SqlCommand(
                "INSERT INTO HoaDon (MaPhong, GioVao, TrangThai) VALUES (@MaPhong, GETDATE(), 0);",
                connection, transaction);
            insert.Parameters.AddWithValue("@MaPhong", roomId);
            insert.ExecuteNonQuery();

            using var update = new SqlCommand(
                "UPDATE PhongHat SET TrangThai = N'Đang hát' WHERE MaPhong = @MaPhong;",
                connection, transaction);
            update.Parameters.AddWithValue("@MaPhong", roomId);
            update.ExecuteNonQuery();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void AddService(int roomId, int serviceId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var invoiceCommand = new SqlCommand(
                "SELECT TOP (1) MaHD FROM HoaDon WHERE MaPhong = @MaPhong AND TrangThai = 0 ORDER BY MaHD DESC;",
                connection, transaction);
            invoiceCommand.Parameters.AddWithValue("@MaPhong", roomId);
            var invoiceId = invoiceCommand.ExecuteScalar();
            if (invoiceId is null)
                throw new InvalidOperationException("Phòng chưa được mở.");

            using var stockCommand = new SqlCommand(
                "SELECT DonGia, TonKho FROM DichVu WHERE MaDV = @MaDV;",
                connection, transaction);
            stockCommand.Parameters.AddWithValue("@MaDV", serviceId);
            using var serviceReader = stockCommand.ExecuteReader();
            if (!serviceReader.Read())
                throw new InvalidOperationException("Không tìm thấy dịch vụ.");
            var unitPrice = serviceReader.GetDecimal(0);
            var stock = serviceReader.GetInt32(1);
            serviceReader.Close();

            using var existingCommand = new SqlCommand(
                "SELECT SoLuong FROM ChiTietDichVu WHERE MaHD = @MaHD AND MaDV = @MaDV;",
                connection, transaction);
            existingCommand.Parameters.AddWithValue("@MaHD", invoiceId);
            existingCommand.Parameters.AddWithValue("@MaDV", serviceId);
            var existing = existingCommand.ExecuteScalar();
            var currentQuantity = existing is null ? 0 : Convert.ToInt32(existing);
            if (currentQuantity + quantity > stock)
                throw new InvalidOperationException($"Tồn kho không đủ (còn {stock}).");

            if (existing is null)
            {
                using var insert = new SqlCommand(
                    "INSERT INTO ChiTietDichVu (MaHD, MaDV, SoLuong, DonGia) VALUES (@MaHD, @MaDV, @SoLuong, @DonGia);",
                    connection, transaction);
                AddDetailParameters(insert, invoiceId, serviceId, quantity, unitPrice);
                insert.ExecuteNonQuery();
            }
            else
            {
                using var update = new SqlCommand(
                    "UPDATE ChiTietDichVu SET SoLuong = SoLuong + @SoLuong WHERE MaHD = @MaHD AND MaDV = @MaDV;",
                    connection, transaction);
                update.Parameters.AddWithValue("@MaHD", invoiceId);
                update.Parameters.AddWithValue("@MaDV", serviceId);
                update.Parameters.AddWithValue("@SoLuong", quantity);
                update.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public CheckoutResult Checkout(int roomId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            const string invoiceSql = """
                SELECT TOP (1) h.MaHD, h.GioVao, p.GiaTheoGio
                FROM HoaDon h WITH (UPDLOCK)
                INNER JOIN PhongHat p ON p.MaPhong = h.MaPhong
                WHERE h.MaPhong = @MaPhong AND h.TrangThai = 0
                ORDER BY h.MaHD DESC;
                """;
            using var invoiceCommand = new SqlCommand(invoiceSql, connection, transaction);
            invoiceCommand.Parameters.AddWithValue("@MaPhong", roomId);
            using var invoiceReader = invoiceCommand.ExecuteReader();
            if (!invoiceReader.Read())
                throw new InvalidOperationException("Phòng chưa có hóa đơn đang mở.");
            var invoiceId = invoiceReader.GetInt32(0);
            var startTime = invoiceReader.GetDateTime(1);
            var hourlyRate = invoiceReader.GetDecimal(2);
            invoiceReader.Close();

            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            var billableHours = Math.Max(1m, (decimal)duration.TotalHours);
            var roomTotal = Math.Round(billableHours * hourlyRate, 2);

            using var serviceTotalCommand = new SqlCommand(
                "SELECT COALESCE(SUM(SoLuong * DonGia), 0) FROM ChiTietDichVu WHERE MaHD = @MaHD;",
                connection, transaction);
            serviceTotalCommand.Parameters.AddWithValue("@MaHD", invoiceId);
            var serviceTotal = Convert.ToDecimal(serviceTotalCommand.ExecuteScalar());

            using var detailCommand = new SqlCommand(
                "SELECT MaDV, SoLuong FROM ChiTietDichVu WHERE MaHD = @MaHD;",
                connection, transaction);
            detailCommand.Parameters.AddWithValue("@MaHD", invoiceId);
            using var detailReader = detailCommand.ExecuteReader();
            var details = new List<(int ServiceId, int Quantity)>();
            while (detailReader.Read())
                details.Add((detailReader.GetInt32(0), detailReader.GetInt32(1)));
            detailReader.Close();

            foreach (var detail in details)
            {
                using var stockUpdate = new SqlCommand(
                    "UPDATE DichVu SET TonKho = TonKho - @SoLuong WHERE MaDV = @MaDV AND TonKho >= @SoLuong;",
                    connection, transaction);
                stockUpdate.Parameters.AddWithValue("@SoLuong", detail.Quantity);
                stockUpdate.Parameters.AddWithValue("@MaDV", detail.ServiceId);
                if (stockUpdate.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Tồn kho không đủ để thanh toán hóa đơn.");
            }

            using var invoiceUpdate = new SqlCommand("""
                UPDATE HoaDon
                SET GioRa = @GioRa, TongTienPhong = @TongTienPhong,
                    TongTienDichVu = @TongTienDichVu, TrangThai = 1
                WHERE MaHD = @MaHD;
                """, connection, transaction);
            invoiceUpdate.Parameters.AddWithValue("@GioRa", endTime);
            invoiceUpdate.Parameters.AddWithValue("@TongTienPhong", roomTotal);
            invoiceUpdate.Parameters.AddWithValue("@TongTienDichVu", serviceTotal);
            invoiceUpdate.Parameters.AddWithValue("@MaHD", invoiceId);
            invoiceUpdate.ExecuteNonQuery();

            using var roomUpdate = new SqlCommand(
                "UPDATE PhongHat SET TrangThai = N'Trống' WHERE MaPhong = @MaPhong;",
                connection, transaction);
            roomUpdate.Parameters.AddWithValue("@MaPhong", roomId);
            roomUpdate.ExecuteNonQuery();
            transaction.Commit();
            return new CheckoutResult(roomTotal, serviceTotal, roomTotal + serviceTotal, duration);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void AddDetailParameters(SqlCommand command, object invoiceId, int serviceId, int quantity, decimal unitPrice)
    {
        command.Parameters.AddWithValue("@MaHD", invoiceId);
        command.Parameters.AddWithValue("@MaDV", serviceId);
        command.Parameters.AddWithValue("@SoLuong", quantity);
        command.Parameters.AddWithValue("@DonGia", unitPrice);
    }
}
