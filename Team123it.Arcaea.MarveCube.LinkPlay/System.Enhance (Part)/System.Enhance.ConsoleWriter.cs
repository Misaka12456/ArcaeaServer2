using System.Text;

namespace System.Enhance
{
	public class ConsoleWriter : TextWriter, IDisposable
	{
		public override Encoding Encoding => Encoding.UTF8;

		public event EventHandler<TextEventArgs>? OnOutput;

		public string Tag { get; set; }

		public ConsoleWriter()
		{
			Tag = string.Empty;
			NewLine = Environment.NewLine;
		}

		public override void Write(bool value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(char value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(char[]? buffer)
		{
			if (buffer != null)
			{
				string r = new string(buffer);
				OnOutput?.Invoke(this, r);
			}
			else
			{
				OnOutput?.Invoke(this, string.Empty);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (index >= buffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			else
			{
				string r = new string(buffer, index, count);
				OnOutput?.Invoke(this, r);
			}
		}

		public override void Write(decimal value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(double value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(int value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(long value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(object? value)
		{
			if (value != null)
			{
				if (value is string r)
				{
					OnOutput?.Invoke(this, r);
				}
				else if (value is double d)
				{
					OnOutput?.Invoke(this, d.ToString());
				}
				else if (value is int i)
				{
					OnOutput?.Invoke(this, i.ToString());
				}
				else if (value is long l)
				{
					OnOutput?.Invoke(this, l.ToString());
				}
				else
				{
					OnOutput?.Invoke(this, !string.IsNullOrEmpty(value.ToString()) ? value.ToString()! : string.Empty);
				}
			}
			else
			{
				OnOutput?.Invoke(this, string.Empty);
			}
		}

		public override void Write(ReadOnlySpan<char> buffer)
		{
			OnOutput?.Invoke(this, new string(buffer));
		}

		public override void Write(float value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(string? value)
		{
			OnOutput?.Invoke(this, !string.IsNullOrEmpty(value) ? value! : string.Empty);
		}

		public override void Write(string format, object? arg0)
		{
			OnOutput?.Invoke(this, string.Format(format, arg0));
		}

		public override void Write(string format, object? arg0, object? arg1)
		{
			OnOutput?.Invoke(this, string.Format(format, arg0, arg1));
		}

		public override void Write(string format, object? arg0, object? arg1, object? arg2)
		{
			OnOutput?.Invoke(this, string.Format(format, arg0, arg1, arg2));
		}

		public override void Write(string format, params object?[] arg)
		{
			OnOutput?.Invoke(this, string.Format(format, arg));
		}

		public override void Write(StringBuilder? value)
		{
			if (value != null)
			{
				OnOutput?.Invoke(this, value.ToString());
			}
			else
			{
				OnOutput?.Invoke(this, string.Empty);
			}
		}

		public override void Write(uint value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override void Write(ulong value)
		{
			OnOutput?.Invoke(this, value.ToString());
		}

		public override Task WriteAsync(char value)
		{
			return Task.Run(() =>
			{
				Write(value);
			});
		}

		public override Task WriteAsync(char[] buffer, int index, int count)
		{
			return Task.Run(() =>
			{
				Write(buffer, index, count);
			});
		}

		public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
		{
			return Task.Run(() =>
			{
				Write(new string(buffer.ToArray()));
			}, cancellationToken);
		}

		public override Task WriteAsync(string? value)
		{
			return Task.Run(() =>
			{
				Write(value);
			});
		}

		public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
		{
			return Task.Run(() =>
			{
				Write(value);
			}, cancellationToken);
		}

		public override void WriteLine()
		{
			Write(NewLine);
		}

		public override void WriteLine(bool value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(char value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(char[]? buffer)
		{
			Write(buffer);
			Write(NewLine);
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			Write(buffer, index, count);
			Write(NewLine);
		}

		public override void WriteLine(decimal value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(double value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(int value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(long value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(object? value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(ReadOnlySpan<char> buffer)
		{
			Write(buffer);
			Write(NewLine);
		}

		public override void WriteLine(float value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(string? value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(string format, object? arg0)
		{
			Write(format, arg0);
			Write(NewLine);
		}

		public override void WriteLine(string format, object? arg0, object? arg1)
		{
			Write(format, arg0, arg1);
			Write(NewLine);
		}

		public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
		{
			Write(format, arg0, arg1, arg2);
			Write(NewLine);
		}

		public override void WriteLine(string format, params object?[] arg)
		{
			Write(format, arg);
			Write(NewLine);
		}

		public override void WriteLine(StringBuilder? value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(uint value)
		{
			Write(value);
			Write(NewLine);
		}

		public override void WriteLine(ulong value)
		{
			Write(value);
			Write(NewLine);
		}

		public override Task WriteLineAsync()
		{
			return Task.Run(() => WriteLine());
		}

		public override Task WriteLineAsync(char value)
		{
			return Task.Run(() => WriteLine(value));
		}

		public override Task WriteLineAsync(char[] buffer, int index, int count)
		{
			return Task.Run(() => WriteLine(buffer, index, count));
		}

		public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
		{
			return Task.Run(() => WriteLine(buffer), cancellationToken);
		}

		public override Task WriteLineAsync(string? value)
		{
			return Task.Run(() => WriteLine(value));
		}

		public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
		{
			return Task.Run(() => WriteLine(value), cancellationToken);
		}

		public override void Close()
		{
			Dispose();
		}

		public new void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public override ValueTask DisposeAsync()
		{
			GC.SuppressFinalize(this);
			Dispose();
			return ValueTask.CompletedTask;
		}

		public override void Flush()
		{

		}

		public override Task FlushAsync()
		{
			return Task.CompletedTask;
		}

	}

	public class TextEventArgs : EventArgs
	{
		public string Text;
		public TextEventArgs(string text = "")
		{
			Text = text;
		}

		public static implicit operator TextEventArgs(string text)
		{
			return new TextEventArgs(text);
		}
	}
}
