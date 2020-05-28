using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASTrace
{
    public partial class Form1 : Form
    {
        const string prompt = "Нажмите для ввода доменного имени или IP адреса";
        private ASTracer tracer = new ASTracer();

        public Form1()
        {
            InitializeComponent();            
            textBox1.Text = prompt;
            textBox1.SelectionStart = 0;
            dataGridView1.Hide();
            pictureBox1.Hide();
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (textBox1.Text == "" || textBox1.Text == prompt)
                textBox1.Text = "";
            textBox1.ForeColor = SystemColors.WindowText;
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.ForeColor = SystemColors.WindowFrame;
                textBox1.Text = prompt;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != prompt)
            { 
                dataGridView1.Hide();
                pictureBox1.Show();
                label1.Text = "Подождите...\nИдет трассировка до:\n" + ASTracer.GetAdressText(textBox1.Text);
                dataGridView1.Rows.Clear();
                var tracered = await traceAS();
                if (tracered == null) 
                { 
                    MessageBox.Show("Введите корректное доменное имя\nили IP адрес");
                    pictureBox1.Hide();
                    label1.Text = "";
                    return;
                }
                foreach (var s in tracered)
                {
                    var data = s.Split(' ');
                    var n = new DataGridViewTextBoxCell() { Value = data[0] };
                    var ip = new DataGridViewTextBoxCell() { Value = data[1] };
                    var asn = new DataGridViewTextBoxCell() { Value = data[2] };
                    var row = new DataGridViewRow();
                    row.Cells.AddRange(n, ip, asn);
                    dataGridView1.Rows.Add(row);
                }
                pictureBox1.Hide();
                dataGridView1.Show();
                if (tracer.TracedToEnd)
                    label1.Text = "Готово\nТрассировка до\n" + ASTracer.GetAdressText(textBox1.Text) + "\nбыла успешно завершена";
                else
                    label1.Text = "Трассировка до\n"+ ASTracer.GetAdressText(textBox1.Text) +"\nне была закончена";
            }
        }

        async Task<String[]> traceAS()
        {
            var tracered = await Task.Run(() => tracer.Trace(textBox1.Text));
            return tracered;
        }
    }
}
