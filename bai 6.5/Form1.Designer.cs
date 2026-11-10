namespace bai_6._5
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private GroupBox roomGroupBox;
        private FlowLayoutPanel roomFlowPanel;
        private GroupBox orderGroupBox;
        private ComboBox serviceComboBox;
        private NumericUpDown quantityNumericUpDown;
        private Button addServiceButton;
        private Button checkoutButton;
        private DataGridView orderedServicesGridView;
        private Label selectedRoomLabel;
        private Label roomTotalLabel;
        private Label serviceTotalLabel;
        private Label grandTotalLabel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            roomGroupBox = new GroupBox();
            roomFlowPanel = new FlowLayoutPanel();
            orderGroupBox = new GroupBox();
            serviceComboBox = new ComboBox();
            quantityNumericUpDown = new NumericUpDown();
            addServiceButton = new Button();
            checkoutButton = new Button();
            orderedServicesGridView = new DataGridView();
            selectedRoomLabel = new Label();
            roomTotalLabel = new Label();
            serviceTotalLabel = new Label();
            grandTotalLabel = new Label();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            roomGroupBox.SuspendLayout();
            orderGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)quantityNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)orderedServicesGridView).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();

            roomGroupBox.Controls.Add(roomFlowPanel);
            roomGroupBox.Location = new Point(12, 12);
            roomGroupBox.Name = "roomGroupBox";
            roomGroupBox.Size = new Size(430, 530);
            roomGroupBox.TabIndex = 0;
            roomGroupBox.TabStop = false;
            roomGroupBox.Text = "Sơ đồ phòng hát (xanh: trống, đỏ: đang hát)";
            roomFlowPanel.AutoScroll = true;
            roomFlowPanel.Dock = DockStyle.Fill;
            roomFlowPanel.Padding = new Padding(10);
            roomFlowPanel.TabIndex = 0;

            orderGroupBox.Controls.Add(selectedRoomLabel);
            orderGroupBox.Controls.Add(serviceComboBox);
            orderGroupBox.Controls.Add(quantityNumericUpDown);
            orderGroupBox.Controls.Add(addServiceButton);
            orderGroupBox.Controls.Add(orderedServicesGridView);
            orderGroupBox.Controls.Add(roomTotalLabel);
            orderGroupBox.Controls.Add(serviceTotalLabel);
            orderGroupBox.Controls.Add(grandTotalLabel);
            orderGroupBox.Controls.Add(checkoutButton);
            orderGroupBox.Location = new Point(458, 12);
            orderGroupBox.Name = "orderGroupBox";
            orderGroupBox.Size = new Size(700, 530);
            orderGroupBox.TabIndex = 1;
            orderGroupBox.TabStop = false;
            orderGroupBox.Text = "Gọi dịch vụ và thanh toán";

            selectedRoomLabel.AutoSize = true;
            selectedRoomLabel.Location = new Point(18, 30);
            selectedRoomLabel.Text = "Chưa chọn phòng";
            selectedRoomLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            serviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            serviceComboBox.Location = new Point(18, 62);
            serviceComboBox.Size = new Size(270, 23);
            quantityNumericUpDown.Location = new Point(300, 62);
            quantityNumericUpDown.Minimum = 1;
            quantityNumericUpDown.Maximum = 100;
            quantityNumericUpDown.Value = 1;
            quantityNumericUpDown.Size = new Size(70, 23);
            addServiceButton.Location = new Point(385, 60);
            addServiceButton.Size = new Size(130, 27);
            addServiceButton.Text = "Thêm dịch vụ";
            addServiceButton.Click += addServiceButton_Click;
            orderedServicesGridView.AllowUserToAddRows = false;
            orderedServicesGridView.AllowUserToDeleteRows = false;
            orderedServicesGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            orderedServicesGridView.ReadOnly = true;
            orderedServicesGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            orderedServicesGridView.Location = new Point(18, 105);
            orderedServicesGridView.Size = new Size(660, 300);
            roomTotalLabel.AutoSize = true;
            roomTotalLabel.Location = new Point(18, 422);
            roomTotalLabel.Text = "Tiền phòng: 0 đ";
            serviceTotalLabel.AutoSize = true;
            serviceTotalLabel.Location = new Point(220, 422);
            serviceTotalLabel.Text = "Tiền dịch vụ: 0 đ";
            grandTotalLabel.AutoSize = true;
            grandTotalLabel.Location = new Point(440, 422);
            grandTotalLabel.Text = "Tổng: 0 đ";
            grandTotalLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            checkoutButton.Location = new Point(18, 460);
            checkoutButton.Size = new Size(150, 35);
            checkoutButton.Text = "Thanh toán";
            checkoutButton.Click += checkoutButton_Click;

            statusStrip.Items.Add(statusLabel);
            statusLabel.Text = "Sẵn sàng";
            ClientSize = new Size(1170, 575);
            Controls.Add(roomGroupBox);
            Controls.Add(orderGroupBox);
            Controls.Add(statusStrip);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hệ thống quản lý đặt phòng Karaoke";
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;

            roomGroupBox.ResumeLayout(false);
            orderGroupBox.ResumeLayout(false);
            orderGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)quantityNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)orderedServicesGridView).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
