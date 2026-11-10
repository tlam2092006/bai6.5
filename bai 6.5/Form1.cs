using System.Data;

namespace bai_6._5
{
    public partial class Form1 : Form
    {
        private readonly KaraokeDatabase database = new();
        private Room? selectedRoom;
        private DataTable? orderedServices;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadServices();
                LoadRooms();
                SetStatus("Đã kết nối KaraokeDB.");
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message);
                MessageBox.Show(this,
                    "Không thể tải dữ liệu. Hãy chạy file KaraokeDB.sql trên SQL Server localhost trước.\n\n" + exception.Message,
                    "Lỗi kết nối cơ sở dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadServices()
        {
            serviceComboBox.DataSource = database.GetServices();
            serviceComboBox.DisplayMember = nameof(ServiceItem.Name);
            serviceComboBox.ValueMember = nameof(ServiceItem.Id);
        }

        private void LoadRooms()
        {
            roomFlowPanel.Controls.Clear();
            foreach (var room in database.GetRooms())
            {
                var roomButton = new Button
                {
                    Tag = room,
                    Text = $"{room.Name}\n{room.HourlyRate:N0} đ/giờ",
                    Width = 185,
                    Height = 85,
                    Margin = new Padding(8),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = room.Status == "Trống" ? Color.SeaGreen : Color.IndianRed,
                    FlatStyle = FlatStyle.Flat
                };
                roomButton.Click += RoomButton_Click;
                roomFlowPanel.Controls.Add(roomButton);
            }
        }

        private void RoomButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button { Tag: Room room })
                return;

            selectedRoom = room;
            selectedRoomLabel.Text = $"Đang chọn: {room.Name} ({room.Status})";
            addServiceButton.Enabled = room.Status == "Đang hát";
            checkoutButton.Enabled = room.Status == "Đang hát";
            if (room.Status == "Đang hát")
                LoadOrderedServices();
            else
            {
                orderedServicesGridView.DataSource = null;
                orderedServices = null;
                UpdateTotals();
            }
        }

        private void LoadOrderedServices()
        {
            if (selectedRoom is null)
                return;

            orderedServices = database.GetOrderedServices(selectedRoom.Id);
            orderedServicesGridView.DataSource = orderedServices;
            if (orderedServicesGridView.Columns[nameof(ServiceItem.Id)] is not null)
                orderedServicesGridView.Columns[nameof(ServiceItem.Id)].Visible = false;
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal serviceTotal = 0;
            if (orderedServices is not null)
            {
                foreach (DataRow row in orderedServices.Rows)
                    serviceTotal += row.Field<decimal>("ThanhTien");
            }

            decimal roomTotal = 0;
            if (selectedRoom is not null && selectedRoom.Status == "Đang hát")
            {
                var invoice = database.GetActiveInvoice(selectedRoom.Id);
                if (invoice is not null)
                {
                    var hours = Math.Max(1m, (decimal)(DateTime.Now - invoice.StartTime).TotalHours);
                    roomTotal = Math.Round(hours * selectedRoom.HourlyRate, 2);
                }
            }

            roomTotalLabel.Text = $"Tiền phòng: {roomTotal:N0} đ";
            serviceTotalLabel.Text = $"Tiền dịch vụ: {serviceTotal:N0} đ";
            grandTotalLabel.Text = $"Tổng: {(roomTotal + serviceTotal):N0} đ";
        }

        private void addServiceButton_Click(object? sender, EventArgs e)
        {
            if (selectedRoom is null || selectedRoom.Status != "Đang hát")
            {
                MessageBox.Show(this, "Hãy chọn một phòng đang hát.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (serviceComboBox.SelectedItem is not ServiceItem service)
                return;

            try
            {
                database.AddService(selectedRoom.Id, service.Id, (int)quantityNumericUpDown.Value);
                LoadOrderedServices();
                SetStatus($"Đã thêm {service.Name} vào {selectedRoom.Name}.");
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private void checkoutButton_Click(object? sender, EventArgs e)
        {
            if (selectedRoom is null || selectedRoom.Status != "Đang hát")
                return;

            var answer = MessageBox.Show(this, $"Thanh toán phòng {selectedRoom.Name}?", "Xác nhận thanh toán",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
                return;

            try
            {
                var roomName = selectedRoom.Name;
                var result = database.Checkout(selectedRoom.Id);
                MessageBox.Show(this,
                    $"Đã thanh toán {roomName}.\nThời gian: {result.Duration:hh\\:mm\\:ss}\nTổng tiền: {result.GrandTotal:N0} đ",
                    "Thanh toán thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedRoom = null;
                selectedRoomLabel.Text = "Chưa chọn phòng";
                orderedServicesGridView.DataSource = null;
                addServiceButton.Enabled = false;
                checkoutButton.Enabled = false;
                LoadRooms();
                UpdateTotals();
                SetStatus("Thanh toán thành công.");
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // The database class creates and disposes each connection per operation.
        }

        private void SetStatus(string message) => statusLabel.Text = message;

        private void ShowError(Exception exception)
        {
            SetStatus(exception.Message);
            MessageBox.Show(this, exception.Message, "Lỗi thao tác", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
