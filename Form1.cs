using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

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
			SetupKeyboardHandling();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			historyBox.Text = "";
		}

		// Setup keyboard input handling
		private void SetupKeyboardHandling()
		{
			this.KeyPreview = true;
			this.KeyDown += Form1_KeyDown;
			this.KeyPress += Form1_KeyPress;
			this.TabStop = true;
			this.Focus();
		}

		// keyboard input handling
		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsDigit(e.KeyChar))
			{
				NumberClick(e.KeyChar.ToString());
				e.Handled = true;
			}
			else if (e.KeyChar == '+')
			{
				OperationClick("+");
				e.Handled = true;
			}
			else if (e.KeyChar == '-')
			{
				OperationClick("-");
				e.Handled = true;
			}
			else if (e.KeyChar == '*')
			{
				OperationClick("×");
				e.Handled = true;
			}
			else if (e.KeyChar == '/')
			{
				OperationClick("÷");
				e.Handled = true;
			}
			else if (e.KeyChar == '.')
			{
				DecimalClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyChar == '(')
			{
				OpenParenthesesClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyChar == ')')
			{
				CloseParenthesesClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyChar == '^')
			{
				OperationClick("^");
				e.Handled = true;
			}
			else if (e.KeyChar == '=')
			{
				EqualsClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyChar == '\r')
			{
				EqualsClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyChar == '\b')
			{
				BackspaceClick(null, null);
				e.Handled = true;
			}
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
			{
				string number = ((int)(e.KeyCode - Keys.NumPad0)).ToString();
				NumberClick(number);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Add)
			{
				OperationClick("+");
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Subtract)
			{
				OperationClick("-");
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Multiply)
			{
				OperationClick("×");
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Divide)
			{
				OperationClick("÷");
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Decimal)
			{
				DecimalClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter)
			{
				EqualsClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
			{
				BackspaceClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Escape)
			{
				ClearClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Delete && e.Control)
			{
				ClearEntryClick(null, null);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.R)
			{
				OperationClick("√");
				e.Handled = true;
			}
		}

		// Wire up button events
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

		// Button click handlers
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

		// Operation handler
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

		// Parentheses handlers
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

		// Equals handler
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

		// Expression evaluation
		private decimal EvaluateExpression(string expression)
		{
			expression = expression.Trim();

			// Handle square root operations first (second number ^ (1/first number))
			expression = ProcessSquareRoots(expression);

			// Handle power operations (first number ^ second number)
			expression = ProcessPowers(expression);

			// Replace remaining operators for DataTable.Compute
			expression = expression.Replace("×", "*").Replace("÷", "/");

			var table = new System.Data.DataTable();
			var result = table.Compute(expression, null);

			if (result == DBNull.Value)
				throw new InvalidOperationException("Invalid expression");

			return Convert.ToDecimal(result);
		}

		private string ProcessSquareRoots(string expression)
		{
			// Pattern: number √ number (first number is the root, second number is the base)
			var rootPattern = @"(\d+(?:\.\d+)?)\s*√\s*(\d+(?:\.\d+)?)";

			while (Regex.IsMatch(expression, rootPattern))
			{
				expression = Regex.Replace(expression, rootPattern, match =>
				{
					decimal rootValue = decimal.Parse(match.Groups[1].Value);
					decimal baseValue = decimal.Parse(match.Groups[2].Value);

					// Calculate: baseValue ^ (1/rootValue)
					double result = Math.Pow((double)baseValue, 1.0 / (double)rootValue);
					return result.ToString();
				});
			}

			return expression;
		}

		private string ProcessPowers(string expression)
		{
			// Pattern: number ^ number (first number raised to the power of second number)
			var powerPattern = @"(\d+(?:\.\d+)?)\s*\^\s*(\d+(?:\.\d+)?)";

			while (Regex.IsMatch(expression, powerPattern))
			{
				expression = Regex.Replace(expression, powerPattern, match =>
				{
					decimal baseValue = decimal.Parse(match.Groups[1].Value);
					decimal exponentValue = decimal.Parse(match.Groups[2].Value);

					// Calculate: baseValue ^ exponentValue
					double result = Math.Pow((double)baseValue, (double)exponentValue);
					return result.ToString();
				});
			}

			return expression;
		}

		// Format result for display
		private string FormatResult(decimal result)
		{
			if (result == Math.Floor(result) && result <= decimal.MaxValue)
				return result.ToString("0");
			else
				return result.ToString("0.##########");
		}

		// Other button handlers
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

		// Clear and Clear Entry handlers
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

				historyBox.Text = "7355608 × 69 ÷ 420 = suprise";
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
				
			}
		}
	}
}