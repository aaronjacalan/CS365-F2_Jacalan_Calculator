using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CS365___Calculator
{
	public partial class Form1 : Form
	{
		private bool isDragging = false;
		private Point startPoint = new Point(0, 0);
		private string currentExpression = "";
		private bool newInput = true;
		private bool hasDecimal = false;
		private int openParentheses = 0;
		private string easterEggSequence = "";

		public Form1()
		{
			InitializeComponent();
			WireUpEvents();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			historyBox.Text = "";
		}

		private void WireUpEvents()
		{
			// Numbers
			btnZero.Click += (s, e) => NumberClick("0");
			btnOne.Click += (s, e) => NumberClick("1");
			btnTwo.Click += (s, e) => NumberClick("2");
			btnThree.Click += (s, e) => NumberClick("3");
			btnFour.Click += (s, e) => NumberClick("4");
			btnFive.Click += (s, e) => NumberClick("5");
			btnSix.Click += (s, e) => NumberClick("6");
			btnSeven.Click += (s, e) => NumberClick("7");
			btnEight.Click += (s, e) => NumberClick("8");
			btnNine.Click += (s, e) => NumberClick("9");

			// Operations
			btnAdd.Click += (s, e) => OperationClick("+");
			btnSubtract.Click += (s, e) => OperationClick("-");
			btnMultiply.Click += (s, e) => OperationClick("×");
			btnDivide.Click += (s, e) => OperationClick("÷");

			// Functions
			btnEquals.Click += EqualsClick;
			btnDecimal.Click += DecimalClick;
			btnOpenParenthesis.Click += OpenParenthesesClick;
			btnCloseParenthesis.Click += CloseParenthesesClick;
			btnPower.Click += (s, e) => OperationClick("^");
			btnNthRoot.Click += (s, e) => OperationClick("√");
			btnBackspace.Click += BackspaceClick;
			btnClear.Click += ClearClick;
			btnClearEntry.Click += ClearEntryClick;
		}

		private void NumberClick(string number)
		{
			TrackEasterEggInput(number);

			if (string.IsNullOrEmpty(currentExpression) || currentExpression == "0")
			{
				currentExpression = number;
			}
			else
			{
				currentExpression += number;
			}

			displayBox.Text = currentExpression;
			newInput = false;
		}

		private void OperationClick(string operation)
		{
			TrackEasterEggInput(operation);
			if (!string.IsNullOrEmpty(currentExpression) && currentExpression != "0")
			{
				currentExpression += " " + operation + " ";
				displayBox.Text = currentExpression;
				newInput = true;
				hasDecimal = false;
			}
		}

		private void OpenParenthesesClick(object sender, EventArgs e)
		{
			ResetEasterEggOnNonSequenceAction();

			if (string.IsNullOrEmpty(currentExpression) || currentExpression == "0")
			{
				currentExpression = "(";
			}
			else
			{
				if (char.IsDigit(currentExpression[currentExpression.Length - 1]) || currentExpression.EndsWith(")"))
				{
					currentExpression += " × (";
				}
				else
				{
					currentExpression += "(";
				}
			}

			openParentheses++;
			displayBox.Text = currentExpression;
			newInput = true;
			hasDecimal = false;
		}

		private void CloseParenthesesClick(object sender, EventArgs e)
		{
			if (openParentheses > 0 && !string.IsNullOrEmpty(currentExpression))
			{
				currentExpression += ")";
				openParentheses--;
				displayBox.Text = currentExpression;
				newInput = true;
				hasDecimal = false;
			}
		}

		private void EqualsClick(object sender, EventArgs e)
		{
			TrackEasterEggInput("=");

			if (!string.IsNullOrEmpty(currentExpression) && currentExpression != "0")
			{
				try
				{
					string originalExpression = currentExpression;
					decimal result = EvaluateExpression(currentExpression);
					historyBox.Text = originalExpression + " =";
					displayBox.Text = FormatResult(result);
					currentExpression = FormatResult(result);
					newInput = true;
					hasDecimal = result.ToString().Contains(".");
				}
				catch (Exception)
				{
					displayBox.Text = "Error";
					historyBox.Text = currentExpression + " = Error";
					currentExpression = "";
					newInput = true;
				}
			}
		}

		private decimal EvaluateExpression(string expression)
		{
			expression = expression.Replace("×", "*").Replace("÷", "/").Replace("^", "^");
			var table = new System.Data.DataTable();
			var result = table.Compute(expression, null);

			if (result == DBNull.Value)
				throw new InvalidOperationException("Invalid expression");

			return Convert.ToDecimal(result);
		}

		private string FormatResult(decimal result)
		{
			if (result == Math.Floor(result) && result <= decimal.MaxValue)
				return result.ToString("0");
			else
				return result.ToString("0.##########");
		}

		private void DecimalClick(object sender, EventArgs e)
		{
			ResetEasterEggOnNonSequenceAction();

			string[] parts = currentExpression.Split(new char[] { '+', '-', '×', '÷', '^', '√', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
			string lastPart = parts.Length > 0 ? parts[parts.Length - 1].Trim() : "";

			if (!hasDecimal && !lastPart.Contains("."))
			{
				if (string.IsNullOrEmpty(currentExpression) || currentExpression == "0")
				{
					currentExpression = "0.";
				}
				else if (char.IsDigit(currentExpression[currentExpression.Length - 1]))
				{
					currentExpression += ".";
				}
				else
				{
					currentExpression += "0.";
				}

				displayBox.Text = currentExpression;
				hasDecimal = true;
			}
		}

		private void BackspaceClick(object sender, EventArgs e)
		{
			ResetEasterEggOnNonSequenceAction();

			if (!string.IsNullOrEmpty(currentExpression) && currentExpression.Length > 0)
			{
				char lastChar = currentExpression[currentExpression.Length - 1];

				if (lastChar == '.')
					hasDecimal = false;

				if (lastChar == '(')
					openParentheses--;
				else if (lastChar == ')')
					openParentheses++;

				currentExpression = currentExpression.Substring(0, currentExpression.Length - 1);

				if (string.IsNullOrEmpty(currentExpression))
				{
					displayBox.Text = "0";
					historyBox.Text = "";
					newInput = true;
				}
				else
				{
					displayBox.Text = currentExpression;
				}
			}
			else
			{
				displayBox.Text = "0";
				currentExpression = "";
				newInput = true;
				hasDecimal = false;
			}
		}

		private void ClearClick(object sender, EventArgs e)
		{
			easterEggSequence = "";
			displayBox.Text = "0";
			historyBox.Text = "";
			currentExpression = "";
			newInput = true;
			hasDecimal = false;
			openParentheses = 0;
		}

		private void ClearEntryClick(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			currentExpression = "";
			newInput = true;
			hasDecimal = false;
		}

		// Dragging the form
		private void Drag_MouseDown(object sender, MouseEventArgs e)
		{
			isDragging = true;
			startPoint = new Point(e.X, e.Y);
		}

		private void Drag_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				Point p = PointToScreen(e.Location);
				Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
			}
		}

		private void Drag_MouseUp(object sender, MouseEventArgs e)
		{
			isDragging = false;
		}

		// Minimize and Close buttons
		private void btnMinimize_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void btnClose_MouseEnter(object sender, EventArgs e)
		{
			btnClose.BackColor = Color.FromArgb(255, 138, 0);
			btnClose.ForeColor = Color.FromArgb(60, 54, 45);
		}

		private void btnClose_MouseLeave(object sender, EventArgs e)
		{
			btnClose.BackColor = Color.Transparent;
			btnClose.ForeColor = Color.White;
		}

		private void btnMinimize_MouseEnter(object sender, EventArgs e)
		{
			btnMinimize.BackColor = Color.FromArgb(152, 43, 237);
			btnMinimize.ForeColor = Color.FromArgb(47, 24, 67);
		}

		private void btnMinimize_MouseLeave(object sender, EventArgs e)
		{
			btnMinimize.BackColor = Color.Transparent;
			btnMinimize.ForeColor = Color.White;
		}

		// Easter Egg
		private void lblTitle_Click(object sender, EventArgs e)
		{
			lblTitle.Text = "input the codes for an easter egg";
		}

		private void TrackEasterEggInput(string input)
		{
			easterEggSequence += input;

			if (easterEggSequence.Contains("7355608×69÷420="))
			{
				easterEggSequence = "";
				TriggerEasterEgg();
			}

			if (easterEggSequence.Length > 50)
			{
				easterEggSequence = easterEggSequence.Substring(easterEggSequence.Length - 50);
			}
		}

		private void ResetEasterEggOnNonSequenceAction()
		{
			easterEggSequence = "";
		}

		private void TriggerEasterEgg()
		{
			try
			{
				decimal step1 = 7355608 * 69;
				decimal result = step1 / 420;

				historyBox.Text = "7355608 × 69 ÷ 420 = Easter Egg!";
				displayBox.Text = FormatResult(result);

				OpenEasterEggLink();
			}
			catch (Exception)
			{
				lblTitle.Text = "Easter Egg Activated!";
				OpenEasterEggLink();
			}
		}

		private void OpenEasterEggLink()
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "https://www.youtube.com/watch?v=uaui_lt5LtQ",
					UseShellExecute = true
				});
			}
			catch (Exception)
			{
				MessageBox.Show("Easter Egg Activated! 🎉", "Surprise!", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}