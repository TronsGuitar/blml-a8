using System;
using System.Windows.Forms;

public class SampleForm : Form
{
    private Button Command1;
    private TextBox Text1;

    public SampleForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Sample Form";
        this.Command1 = new Button();
        this.Command1.Text = "Run";
        this.Command1.Left = 150;
        this.Command1.Top = 300;
        this.Controls.Add(this.Command1);
        this.Text1 = new TextBox();
        this.Text1.Left = 150;
        this.Text1.Top = 900;
        this.Controls.Add(this.Text1);
    }
}
