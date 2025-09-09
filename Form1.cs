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

		private decimal currentValue = 0;
		private decimal previousValue = 0;
		private string currentOperation = "";
		private bool operationPending = false;
		private bool newInput = true;
		private bool hasDecimal = false;

		// needed for parentheses functionality
		private Stack<decimal> valueStack = new Stack<decimal>();
		private Stack<string> operatorStack = new Stack<string>();
		private Stack<bool> operationPendingStack = new Stack<bool>();
		private int openParentheses = 0;

		// Easter egg tracking - enhanced to require full input sequence
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

			// para magamit ang keyboard
			this.KeyPreview = true;
			this.KeyPress += Form1_KeyPress;
		}

		private void NumberClick(string number)
		{
			// Easter egg tracking - track numbers
			TrackEasterEggInput(number);

			if (newInput)
			{
				displayBox.Text = number;
				newInput = false;
			}
			else
			{
				if (displayBox.Text == "0" || displayBox.Text == "(")
					displayBox.Text = number;
				else
					displayBox.Text += number;
			}
		}

		private void OperationClick(string operation)
		{
			// Track operations for easter egg
			TrackEasterEggInput(operation);

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
			// Reset easter egg sequence for operations that break the flow
			ResetEasterEggOnNonSequenceAction();

			// Store current state when opening parentheses
			valueStack.Push(previousValue);
			operatorStack.Push(currentOperation);
			operationPendingStack.Push(operationPending);

			// Reset current state for new parentheses group
			previousValue = 0;
			currentOperation = "";
			operationPending = false;
			openParentheses++;

			// Show opening parenthesis in display box
			displayBox.Text = "(";
			historyBox.Text += "(";
			newInput = true;
			hasDecimal = false;
		}

		private void CloseParenthesesClick(object sender, EventArgs e)
		{
			if (openParentheses > 0 && valueStack.Count > 0)
			{
				// Calculate current expression result if there's a pending operation
				if (operationPending)
				{
					currentValue = decimal.Parse(displayBox.Text);
					CalculateResult();
				}

				// Get the result of the parentheses group
				decimal parenthesesResult = decimal.Parse(displayBox.Text);

				// Restore previous state
				decimal storedPreviousValue = valueStack.Pop();
				string storedOperation = operatorStack.Pop();
				bool storedOperationPending = operationPendingStack.Pop();

				// Apply the stored operation with the parentheses result
				if (storedOperationPending && !string.IsNullOrEmpty(storedOperation))
				{
					previousValue = storedPreviousValue;
					currentValue = parenthesesResult;
					currentOperation = storedOperation;
					CalculateResult();
					operationPending = false;
				}
				else
				{
					displayBox.Text = FormatResult(parenthesesResult);
					previousValue = parenthesesResult;
					operationPending = storedOperationPending;
					currentOperation = storedOperation;
				}

				openParentheses--;
				historyBox.Text += ")";
				newInput = true;
				hasDecimal = false;
			}
		}

		private void EqualsClick(object sender, EventArgs e)
		{
			// Check for easter egg completion on equals
			TrackEasterEggInput("=");

			if (operationPending)
			{
				currentValue = decimal.Parse(displayBox.Text);
				CalculateResult();

				historyBox.Text = $"{previousValue} {currentOperation} {currentValue} =";

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
			// Reset easter egg sequence for non-sequence actions
			ResetEasterEggOnNonSequenceAction();

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
		}

		private void BackspaceClick(object sender, EventArgs e)
		{
			// Reset easter egg sequence
			ResetEasterEggOnNonSequenceAction();

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
		}

		private void ClearClick(object sender, EventArgs e)
		{
			// Reset easter egg sequence
			easterEggSequence = "";

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
			operationPendingStack.Clear();
			openParentheses = 0;
		}

		private void ClearEntryClick(object sender, EventArgs e)
		{
			displayBox.Text = "0";
			newInput = true;
			hasDecimal = false;
		}

		private void UpdateHistory()
		{
			if (!string.IsNullOrEmpty(currentOperation))
			{
				historyBox.Text = $"{previousValue} {currentOperation}";
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

		// Enhanced Easter egg functionality
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
				displayBox.Text = "Easter Egg Activated!";
				historyBox.Text = "Easter Egg!";
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