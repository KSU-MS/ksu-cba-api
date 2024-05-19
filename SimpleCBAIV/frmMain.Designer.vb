<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.lblVolts = New System.Windows.Forms.Label()
        Me.lblAmps = New System.Windows.Forms.Label()
        Me.lblCutoff = New System.Windows.Forms.Label()
        Me.lblLoad = New System.Windows.Forms.Label()
        Me.nUDCutoff = New System.Windows.Forms.NumericUpDown()
        Me.nUDLoad = New System.Windows.Forms.NumericUpDown()
        Me.btnSetStatus = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.txtStatus = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblVoltage = New System.Windows.Forms.Label()
        Me.lblAmpHrs = New System.Windows.Forms.Label()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.txtTime = New System.Windows.Forms.TextBox()
        Me.txtAmpHrs = New System.Windows.Forms.TextBox()
        Me.txtVoltage = New System.Windows.Forms.TextBox()
        Me.GroupBox1.SuspendLayout()
        CType(Me.nUDCutoff, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nUDLoad, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.btnStop)
        Me.GroupBox1.Controls.Add(Me.lblVolts)
        Me.GroupBox1.Controls.Add(Me.lblAmps)
        Me.GroupBox1.Controls.Add(Me.lblCutoff)
        Me.GroupBox1.Controls.Add(Me.lblLoad)
        Me.GroupBox1.Controls.Add(Me.nUDCutoff)
        Me.GroupBox1.Controls.Add(Me.nUDLoad)
        Me.GroupBox1.Controls.Add(Me.btnSetStatus)
        Me.GroupBox1.Location = New System.Drawing.Point(3, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(170, 124)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "User Input"
        '
        'btnStop
        '
        Me.btnStop.Enabled = False
        Me.btnStop.Location = New System.Drawing.Point(5, 99)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.btnStop.Size = New System.Drawing.Size(159, 21)
        Me.btnStop.TabIndex = 7
        Me.btnStop.Text = "Stop Test"
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'lblVolts
        '
        Me.lblVolts.AutoSize = True
        Me.lblVolts.Location = New System.Drawing.Point(131, 48)
        Me.lblVolts.Name = "lblVolts"
        Me.lblVolts.Size = New System.Drawing.Size(30, 13)
        Me.lblVolts.TabIndex = 6
        Me.lblVolts.Text = "Volts"
        '
        'lblAmps
        '
        Me.lblAmps.AutoSize = True
        Me.lblAmps.Location = New System.Drawing.Point(131, 22)
        Me.lblAmps.Name = "lblAmps"
        Me.lblAmps.Size = New System.Drawing.Size(33, 13)
        Me.lblAmps.TabIndex = 5
        Me.lblAmps.Text = "Amps"
        '
        'lblCutoff
        '
        Me.lblCutoff.AutoSize = True
        Me.lblCutoff.Location = New System.Drawing.Point(2, 48)
        Me.lblCutoff.Name = "lblCutoff"
        Me.lblCutoff.Size = New System.Drawing.Size(38, 13)
        Me.lblCutoff.TabIndex = 4
        Me.lblCutoff.Text = "Cutoff:"
        '
        'lblLoad
        '
        Me.lblLoad.AutoSize = True
        Me.lblLoad.Location = New System.Drawing.Point(2, 22)
        Me.lblLoad.Name = "lblLoad"
        Me.lblLoad.Size = New System.Drawing.Size(34, 13)
        Me.lblLoad.TabIndex = 3
        Me.lblLoad.Text = "Load:"
        '
        'nUDCutoff
        '
        Me.nUDCutoff.DecimalPlaces = 2
        Me.nUDCutoff.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nUDCutoff.Location = New System.Drawing.Point(46, 46)
        Me.nUDCutoff.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nUDCutoff.Name = "nUDCutoff"
        Me.nUDCutoff.Size = New System.Drawing.Size(79, 20)
        Me.nUDCutoff.TabIndex = 2
        '
        'nUDLoad
        '
        Me.nUDLoad.DecimalPlaces = 3
        Me.nUDLoad.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nUDLoad.Location = New System.Drawing.Point(46, 20)
        Me.nUDLoad.Maximum = New Decimal(New Integer() {20, 0, 0, 0})
        Me.nUDLoad.Name = "nUDLoad"
        Me.nUDLoad.Size = New System.Drawing.Size(79, 20)
        Me.nUDLoad.TabIndex = 1
        '
        'btnSetStatus
        '
        Me.btnSetStatus.Enabled = False
        Me.btnSetStatus.Location = New System.Drawing.Point(5, 72)
        Me.btnSetStatus.Name = "btnSetStatus"
        Me.btnSetStatus.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.btnSetStatus.Size = New System.Drawing.Size(159, 21)
        Me.btnSetStatus.TabIndex = 0
        Me.btnSetStatus.Text = "Start/Change Test"
        Me.btnSetStatus.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.txtStatus)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.lblVoltage)
        Me.GroupBox2.Controls.Add(Me.lblAmpHrs)
        Me.GroupBox2.Controls.Add(Me.lblTime)
        Me.GroupBox2.Controls.Add(Me.txtTime)
        Me.GroupBox2.Controls.Add(Me.txtAmpHrs)
        Me.GroupBox2.Controls.Add(Me.txtVoltage)
        Me.GroupBox2.Location = New System.Drawing.Point(3, 133)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(170, 131)
        Me.GroupBox2.TabIndex = 1
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "CBA Status"
        '
        'txtStatus
        '
        Me.txtStatus.AutoSize = True
        Me.txtStatus.Location = New System.Drawing.Point(61, 97)
        Me.txtStatus.Name = "txtStatus"
        Me.txtStatus.Size = New System.Drawing.Size(74, 13)
        Me.txtStatus.TabIndex = 12
        Me.txtStatus.Text = "Test Running:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 97)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(40, 13)
        Me.Label1.TabIndex = 11
        Me.Label1.Text = "Status:"
        '
        'lblVoltage
        '
        Me.lblVoltage.AutoSize = True
        Me.lblVoltage.Location = New System.Drawing.Point(6, 22)
        Me.lblVoltage.Name = "lblVoltage"
        Me.lblVoltage.Size = New System.Drawing.Size(46, 13)
        Me.lblVoltage.TabIndex = 7
        Me.lblVoltage.Text = "Voltage:"
        '
        'lblAmpHrs
        '
        Me.lblAmpHrs.AutoSize = True
        Me.lblAmpHrs.Location = New System.Drawing.Point(6, 48)
        Me.lblAmpHrs.Name = "lblAmpHrs"
        Me.lblAmpHrs.Size = New System.Drawing.Size(47, 13)
        Me.lblAmpHrs.TabIndex = 8
        Me.lblAmpHrs.Text = "AmpHrs:"
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Location = New System.Drawing.Point(6, 74)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(33, 13)
        Me.lblTime.TabIndex = 9
        Me.lblTime.Text = "Time:"
        '
        'txtTime
        '
        Me.txtTime.Location = New System.Drawing.Point(64, 71)
        Me.txtTime.Name = "txtTime"
        Me.txtTime.ReadOnly = True
        Me.txtTime.Size = New System.Drawing.Size(97, 20)
        Me.txtTime.TabIndex = 2
        '
        'txtAmpHrs
        '
        Me.txtAmpHrs.Location = New System.Drawing.Point(64, 45)
        Me.txtAmpHrs.Name = "txtAmpHrs"
        Me.txtAmpHrs.ReadOnly = True
        Me.txtAmpHrs.Size = New System.Drawing.Size(97, 20)
        Me.txtAmpHrs.TabIndex = 1
        '
        'txtVoltage
        '
        Me.txtVoltage.Location = New System.Drawing.Point(64, 19)
        Me.txtVoltage.Name = "txtVoltage"
        Me.txtVoltage.ReadOnly = True
        Me.txtVoltage.Size = New System.Drawing.Size(97, 20)
        Me.txtVoltage.TabIndex = 0
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.ClientSize = New System.Drawing.Size(176, 266)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(184, 300)
        Me.MinimizeBox = False
        Me.Name = "frmMain"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.Text = "Simple CBAIV"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.nUDCutoff, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nUDLoad, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents btnSetStatus As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents lblVolts As System.Windows.Forms.Label
    Friend WithEvents lblAmps As System.Windows.Forms.Label
    Friend WithEvents lblCutoff As System.Windows.Forms.Label
    Friend WithEvents lblLoad As System.Windows.Forms.Label
    Friend WithEvents nUDCutoff As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblVoltage As System.Windows.Forms.Label
    Friend WithEvents lblAmpHrs As System.Windows.Forms.Label
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents txtTime As System.Windows.Forms.TextBox
    Friend WithEvents txtAmpHrs As System.Windows.Forms.TextBox
    Friend WithEvents txtVoltage As System.Windows.Forms.TextBox
    Friend WithEvents nUDLoad As System.Windows.Forms.NumericUpDown
    Friend WithEvents txtStatus As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnStop As System.Windows.Forms.Button
End Class
