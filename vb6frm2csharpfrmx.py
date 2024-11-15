import re

# Mapping of VB6 controls to C# equivalents
vb6_to_csharp_controls = {
    "VB.CommandButton": "System.Windows.Forms.Button",
    "VB.TextBox": "System.Windows.Forms.TextBox",
    "VB.Label": "System.Windows.Forms.Label",
    "VB.CheckBox": "System.Windows.Forms.CheckBox",
    "VB.OptionButton": "System.Windows.Forms.RadioButton",
    "VB.ListBox": "System.Windows.Forms.ListBox",
    "VB.ComboBox": "System.Windows.Forms.ComboBox",
    "VB.Frame": "System.Windows.Forms.GroupBox",
    "VB.PictureBox": "System.Windows.Forms.PictureBox",
    "VB.HScrollBar": "System.Windows.Forms.HScrollBar",
    "VB.VScrollBar": "System.Windows.Forms.VScrollBar",
    "VB.Timer": "System.Windows.Forms.Timer"
}

def parse_and_convert_to_csharp(file_path, output_path):
    controls = []
    current_control = None
    
    with open(file_path, 'r') as file:
        for line in file:
            line = line.strip()
            
            # Identify a new control
            if line.startswith('Begin '):
                match = re.match(r'Begin\s+(\w+\.\w+)\s+(\w+)', line)
                if match:
                    vb6_type, control_name = match.groups()
                    controls.append({
                        'Type': vb6_to_csharp_controls.get(vb6_type, vb6_type),
                        'Name': control_name,
                        'Properties': {}
                    })
                    current_control = controls[-1]
            
            # End of control declaration
            elif line == 'End':
                current_control = None
            
            # Capture properties
            elif current_control and '=' in line:
                prop_name, prop_value = line.split('=', 1)
                current_control['Properties'][prop_name.strip()] = prop_value.strip()
    
    # Generate .frmx file
    with open(output_path, 'w') as output_file:
        output_file.write("C# Form\n")
        for control in controls:
            output_file.write(f"Control: {control['Type']} Name: {control['Name']}\n")
            for prop, value in control['Properties'].items():
                output_file.write(f"  {prop} = {value}\n")
            output_file.write("\n")

# Usage
input_file = 'path_to_your_form.frm'
output_file = 'path_to_your_output.frmx'
parse_and_convert_to_csharp(input_file, output_file)

print(f".frmx file generated at {output_file}")