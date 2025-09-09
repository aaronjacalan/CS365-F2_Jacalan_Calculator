using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CS365___Calculator
{
	public partial class Form1 : Form
	{
		private bool isDragging = false;
		private Point startPoint = new Point(0, 0);

		private decimal currentValue = 0;
		private decimal previousValue = 0;
		private string currentOperation = "";
		private bool operationPending = false;
		private bool newInput = true;
		private bool hasDecimal = false;

		// needed for parentheses functionality
		private Stack<decimal> valueStack = new Stack<decimal>();
		private Stack<string> operatorStack = new Stack<string>();
		private int openParentheses = 0;

		public Form1()
		{
			InitializeComponent();
			WireUpEvents();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			historyBox.Text = "";
			AlignBottom(displayBox);
			AlignBottom(historyBox);
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

			// para magamit ang keyboard
			this.KeyPreview = true;
			this.KeyPress += Form1_KeyPress;
		}

		private void NumberClick(string number)
		{
			if (newInput)
			{
				displayBox.Text = number;
				newInput = false;
			}
			else
			{
				if (displayBox.Text == "0")
					displayBox.Text = number;
				else
					displayBox.Text += number;
			}
			AlignBottom(displayBox);
		}

		private void OperationClick(string operation)
		{
			if (operationPending && !newInput)
			{
				CalculateResult();
			}

			previousValue = decimal.Parse(displayBox.Text);
			currentOperation = operation;
			operationPending = true;
			newInput = true;
			hasDecimal = false;

			UpdateHistory();
		}

		private void OpenParenthesesClick(object sender, EventArgs e)
		{
			// Store current state when opening parentheses
			if (operationPending)
			{
				valueStack.Push(previousValue);
				operatorStack.Push(currentOperation);
			}
			else
			{
				valueStack.Push(decimal.Parse(displayBox.Text));
				operatorStack.Push("");
			}

			openParentheses++;
			displayBox.Text = "0";
			historyBox.Text += "(";
			newInput = true;
			hasDecimal = false;
			operationPending = false;
			AlignBottom(displayBox);
			AlignBottom(historyBox);
		}

		private void CloseParenthesesClick(object sender, EventArgs e)
		{
			if (openParentheses > 0 && valueStack.Count > 0)
			{
				// Calculate current expression result
				decimal currentResult = decimal.Parse(displayBox.Text);

				if (operationPending)
				{
					currentValue = currentResult;
					CalculateResult();
					currentResult = decimal.Parse(displayBox.Text);
				}

				// Restore previous state
				decimal storedValue = valueStack.Pop();
				string storedOperation = operatorStack.Pop();

				if (!string.IsNullOrEmpty(storedOperation))
				{
					// Apply the stored operation
					previousValue = storedValue;
					currentValue = currentResult;
					currentOperation = storedOperation;
					CalculateResult();
				}
				else
				{
					displayBox.Text = FormatResult(currentResult);
				}

				openParentheses--;
				historyBox.Text += ")";
				newInput = true;
				hasDecimal = false;
				operationPending = false;
				AlignBottom(displayBox);
				AlignBottom(historyBox);
			}
		}

		private void EqualsClick(object sender, EventArgs e)
		{
			if (operationPending)
			{
				currentValue = decimal.Parse(displayBox.Text);
				CalculateResult();

				historyBox.Text = $"{previousValue} {currentOperation} {currentValue} =";
				AlignBottom(historyBox);

				operationPending = false;
				newInput = true;
				hasDecimal = false;
			}
		}

		private void CalculateResult()
		{
			try
			{
				currentValue = decimal.Parse(displayBox.Text);
				decimal result = 0;

				switch (currentOperation)
				{
					case "+":
						result = previousValue + currentValue;
						break;
					case "-":
						result = previousValue - currentValue;
						break;
					case "×":
						result = previousValue * currentValue;
						break;
					case "÷":
						if (currentValue == 0)
						{
							displayBox.Text = "Cannot divide by zero";
							return;
						}
						result = previousValue / currentValue;
						break;
					case "^":
						result = (decimal)Math.Pow((double)previousValue, (double)currentValue);
						break;
					case "√":
						if (currentValue == 0)
						{
							displayBox.Text = "Cannot calculate root with 0";
							return;
						}
						result = (decimal)Math.Pow((double)previousValue, 1.0 / (double)currentValue);
						break;
				}

				displayBox.Text = FormatResult(result);
				previousValue = result;
			}
			catch (Exception)
			{
				displayBox.Text = "Error";
			}

			AlignBottom(displayBox);
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
			if (newInput)
			{
				displayBox.Text = "0.";
				newInput = false;
				hasDecimal = true;
			}
			else if (!hasDecimal)
			{
				displayBox.Text += ".";
				hasDecimal = true;
			}
			AlignBottom(displayBox);
		}

		private void BackspaceClick(object sender, EventArgs e)
		{
			if (!newInput && displayBox.Text.Length > 1)
			{
				if (displayBox.Text[displayBox.Text.Length - 1] == '.')
					hasDecimal = false;

				displayBox.Text = displayBox.Text.Substring(0, displayBox.Text.Length - 1);
			}
			else
			{
				displayBox.Text = "0";
				newInput = true;
				hasDecimal = false;
			}
			AlignBottom(displayBox);
		}

		private void ClearClick(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			historyBox.Text = "";
			currentValue = 0;
			previousValue = 0;
			currentOperation = "";
			operationPending = false;
			newInput = true;
			hasDecimal = false;

			// Clear parentheses stacks
			valueStack.Clear();
			operatorStack.Clear();
			openParentheses = 0;

			AlignBottom(displayBox);
			AlignBottom(historyBox);
		}

		private void ClearEntryClick(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			newInput = true;
			hasDecimal = false;
			AlignBottom(displayBox);
		}

		private void UpdateHistory()
		{
			if (!string.IsNullOrEmpty(currentOperation))
			{
				historyBox.Text = $"{previousValue} {currentOperation}";
				AlignBottom(historyBox);
			}
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			switch (e.KeyChar)
			{
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					NumberClick(e.KeyChar.ToString());
					break;
				case '+':
					OperationClick("+");
					break;
				case '-':
					OperationClick("-");
					break;
				case '*':
					OperationClick("×");
					break;
				case '/':
					OperationClick("÷");
					break;
				case '.':
					DecimalClick(null, null);
					break;
				case '\r': // Enter key
					EqualsClick(null, null);
					break;
				case '\b': // Backspace
					BackspaceClick(null, null);
					break;
				case '\x1B': // Escape key
					ClearClick(null, null);
					break;
				case '(': // Open parentheses
					OpenParenthesesClick(null, null);
					break;
				case ')': // Close parentheses
					CloseParenthesesClick(null, null);
					break;
			}
		}

		private void AlignBottom(RichTextBox box)
		{
			box.SelectionAlignment = HorizontalAlignment.Right;
			box.SelectionStart = box.Text.Length;
			box.ScrollToCaret();
		}

		// Title bar drag functionality
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

		// Window control buttons
		private void btnMinimize_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		// Close button hover effects
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

		// easter eggs
		private void lblTitle_Click(object sender, EventArgs e)
		{
			lblTitle.Text = "Balls";
		}
	}
}