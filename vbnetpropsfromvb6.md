Great, I will research and map each VB6 control-property pair to its closest equivalent in VB.NET WinForms, including the corresponding data type. This will help you transition from VB6 to VB.NET smoothly. I'll update you once I have the full mapping ready.

**Form Controls**  
- **Form.Caption**, *String* → **Form.Text**, *String* ([
	VB Migration Partner - VB6 vs VB.NET - Label control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=14#:~:text=Caption%20property))  
- **Form.BackColor**, *OLE_COLOR (Long)* → **Form.BackColor**, *System.Drawing.Color* ([
	VB Migration Partner - Knowledge base - OLE_COLOR type is converted to System.Drawing.Color
](https://www.vbmigration.com/detknowledgebase.aspx?Id=298#:~:text=The%20OLE_COLOR%20type%20is%20actually,Color%20type))  
- **Form.ForeColor**, *OLE_COLOR (Long)* → **Form.ForeColor**, *System.Drawing.Color* ([
	VB Migration Partner - Knowledge base - OLE_COLOR type is converted to System.Drawing.Color
](https://www.vbmigration.com/detknowledgebase.aspx?Id=298#:~:text=The%20OLE_COLOR%20type%20is%20actually,Color%20type))  
- **Form.Icon**, *Picture (StdPicture)* → **Form.Icon**, *System.Drawing.Icon* ([Intrinsic Control Mappings](https://www.mobilize.net/vbtonet/vbuc-basic-features/intrinsic-control-mappings#:~:text=Vb,Font))

**Label Controls**  
- **Label.Caption**, *String* → **Label.Text**, *String* ([
	VB Migration Partner - VB6 vs VB.NET - Label control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=14#:~:text=Caption%20property))  
- **Label.Alignment**, *Integer* → **Label.TextAlign**, *ContentAlignment (Enum)* ([
	VB Migration Partner - VB6 vs VB.NET - Label control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=14#:~:text=Alignment%20property))

**TextBox Controls**  
- **TextBox.Text**, *String* → **TextBox.Text**, *String* (No change)  
- **TextBox.Alignment**, *Integer* → **TextBox.TextAlign**, *HorizontalAlignment (Enum)* ([
	VB Migration Partner - VB6 vs VB.NET - TextBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=15#:~:text=Alignment%20property))  
- **TextBox.Locked**, *Boolean* → **TextBox.ReadOnly**, *Boolean* ([
	VB Migration Partner - VB6 vs VB.NET - TextBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=15#:~:text=Locked%20property))  
- **TextBox.PasswordChar**, *String* → **TextBox.PasswordChar**, *Char* ([
	VB Migration Partner - VB6 vs VB.NET - TextBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=15#:~:text=PasswordChar%20property))

**Button Controls (CommandButton)**  
- **CommandButton.Caption**, *String* → **Button.Text**, *String* ([
	VB Migration Partner - VB6 vs VB.NET - Label control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=14#:~:text=Caption%20property))  
- **CommandButton.Picture**, *StdPicture (Image)* → **Button.Image**, *System.Drawing.Image* ([
	VB Migration Partner - VB6 vs VB.NET - CommandButton control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=17#:~:text=Picture%20and%20Style%20properties))  
- **CommandButton.Appearance**, *Integer* → **Button.FlatStyle**, *FlatStyle (Enum)* ([
	VB Migration Partner - VB6 vs VB.NET - CommandButton control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=17#:~:text=Appearance%20property))

**CheckBox Controls**  
- **CheckBox.Caption**, *String* → **CheckBox.Text**, *String* ([
	VB Migration Partner - VB6 vs VB.NET - Label control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=14#:~:text=Caption%20property))  
- **CheckBox.Value**, *Boolean/Variant* → **CheckBox.Checked**, *Boolean* ([
	VB Migration Partner - VB6 vs VB.NET - CheckBox and OptionButton controls
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=16#:~:text=Value%20property))  
- **CheckBox.TripleState**, *Boolean* → **CheckBox.ThreeState**, *Boolean* (ThreeState property enables indeterminate state)  
- **CheckBox.Appearance**, *Integer* → **CheckBox.FlatStyle**, *FlatStyle (Enum)* ([
	VB Migration Partner - VB6 vs VB.NET - CheckBox and OptionButton controls
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=16#:~:text=Picture%20and%20Style%20properties))

**OptionButton (RadioButton) Controls**  
- **OptionButton.Caption**, *String* → **RadioButton.Text**, *String* ([
	VB Migration Partner - VB6 vs VB.NET - Label control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=14#:~:text=Caption%20property)) ([Intrinsic Control Mappings](https://www.mobilize.net/vbtonet/vbuc-basic-features/intrinsic-control-mappings#:~:text=Vb,PictureBox))  
- **OptionButton.Value**, *Boolean* → **RadioButton.Checked**, *Boolean* ([
	VB Migration Partner - VB6 vs VB.NET - CheckBox and OptionButton controls
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=16#:~:text=Value%20property)) ([Intrinsic Control Mappings](https://www.mobilize.net/vbtonet/vbuc-basic-features/intrinsic-control-mappings#:~:text=Vb,PictureBox))

**Frame Controls**  
- **Frame.Caption**, *String* → **GroupBox.Text**, *String* ([[INFO] All ListBox and Frame controls in an array must have same style](https://www.vbmigration.com/detknowledgebase.aspx?Id=601#:~:text=style%20www,VB)) ([Intrinsic Control Mappings](https://www.mobilize.net/vbtonet/vbuc-basic-features/intrinsic-control-mappings#:~:text=Vb,Label))

**Image/PictureBox Controls**  
- **PictureBox.Picture**, *StdPicture (Image)* → **PictureBox.Image**, *System.Drawing.Image* ([
	VB Migration Partner - VB6 vs VB.NET - PictureBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=23#:~:text=Picture%20property))  
- **Image.Picture** (VB6 Image control), *StdPicture* → **PictureBox.Image**, *System.Drawing.Image* ([Intrinsic Control Mappings](https://www.mobilize.net/vbtonet/vbuc-basic-features/intrinsic-control-mappings#:~:text=stdole,Label)) ([
	VB Migration Partner - VB6 vs VB.NET - PictureBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=23#:~:text=Picture%20property))

**ListBox Controls**  
- **ListBox.List**, *String array/Variant* → **ListBox.Items**, *ListBox.ObjectCollection* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=List%20property))  
- **ListBox.ListCount**, *Integer* → **ListBox.Items.Count**, *Integer* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=ListCount%20property))  
- **ListBox.ListIndex**, *Integer* → **ListBox.SelectedIndex**, *Integer* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=ListIndex%20property))

**ComboBox Controls**  
- **ComboBox.List**, *String array/Variant* → **ComboBox.Items**, *ComboBox.ObjectCollection* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=List%20property))  
- **ComboBox.ListCount**, *Integer* → **ComboBox.Items.Count**, *Integer* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=ListCount%20property))  
- **ComboBox.ListIndex**, *Integer* → **ComboBox.SelectedIndex**, *Integer* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=ListIndex%20property))  
- **ComboBox.Style**, *Integer* → **ComboBox.DropDownStyle**, *ComboBoxStyle (Enum)* ([
	VB Migration Partner - VB6 vs VB.NET - ComboBox control
](https://www.vbmigration.com/resources/detmigratingfromvb6controls.aspx?Id=21#:~:text=Locked%20property))

**ScrollBar Controls**  
- **HScrollBar.Min**, *Integer* → **HScrollBar.Minimum**, *Integer*  
- **HScrollBar.Max**, *Integer* → **HScrollBar.Maximum**, *Integer*  
- **HScrollBar.Value**, *Integer* → **HScrollBar.Value**, *Integer* (Same property name in .NET)  

**Timer Control**  
- **Timer.Interval**, *Integer* → **Timer.Interval**, *Integer* (Same property name and type)  
- **Timer.Enabled**, *Boolean* → **Timer.Enabled**, *Boolean* (No change)

