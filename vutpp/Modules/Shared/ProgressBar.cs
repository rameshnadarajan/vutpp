using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace VistaStyleProgressBar
{
	/// <summary>
	/// A replacement for the default ProgressBar control.
	/// </summary>
	[DefaultEvent("ValueChanged")]
	public class ProgressBar : System.Windows.Forms.UserControl 
	{

		#region -  Designer  -

			/// <summary> 
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.Container components = null;

			/// <summary>
			/// Create the control and initialize it.
			/// </summary>
			public ProgressBar()
			{
				// This call is required by the Windows.Forms Form Designer.
				InitializeComponent();
				this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				this.SetStyle(ControlStyles.DoubleBuffer, true);
				this.SetStyle(ControlStyles.ResizeRedraw, true);
				this.SetStyle(ControlStyles.Selectable, true);
				this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
				this.SetStyle(ControlStyles.UserPaint, true);
				this.BackColor = Color.Transparent;
				if (!InDesignMode())
				{
					mGlowAnimation.Tick += new EventHandler(mGlowAnimation_Tick);
					mGlowAnimation.Interval = 15;
					if (Value < MaxValue) {StartTimer();}
				}
			}

			/// <summary> 
			/// Clean up any resources being used.
			/// </summary>
			protected override void Dispose( bool disposing )
			{
				if( disposing )
				{
					if(components != null)
					{
						components.Dispose();
					}
				}
				base.Dispose( disposing );
			}

			#region -  Component Designer  -
			
				/// <summary> 
				/// Required method for Designer support - do not modify 
				/// the contents of this method with the code editor.
				/// </summary>
				private void InitializeComponent()
				{
                    this.SuspendLayout();
                    // 
                    // ProgressBar
                    // 
                    this.BackColor = System.Drawing.SystemColors.Control;
                    this.Name = "ProgressBar";
                    this.Size = new System.Drawing.Size(264, 32);
                    this.Paint += new System.Windows.Forms.PaintEventHandler(this.ProgressBar_Paint);
                    this.ResumeLayout(false);

				}

			#endregion

		#endregion

		#region -  Properties  -

			private int mGlowPosition = -325;
            private System.Windows.Forms.Timer mGlowAnimation = new System.Windows.Forms.Timer();

			#region -  Value  -

				private int mValue = 0;
				/// <summary>
				/// The value that is displayed on the progress bar.
				/// </summary>
                [Category("Value"), 
				DefaultValue(0),
				Description("The value that is displayed on the progress bar.")]
				public int Value
				{
					get { return mValue; }
					set 
					{ 
                       SetValue( value );
					}
				}

                delegate void SetValueCB( int value );
                void SetValue(int value)
                {
                    if (this.InvokeRequired == true)
                    {
                        this.Invoke(new SetValueCB(SetValue), new object[] { value });
                        return;
                    }

                    if (value > MaxValue || value < MinValue) { return; }
                    mValue = value;
                    if (value > MinValue && value < MaxValue) { StartTimer(); }
                    else { StopTimer(); }
                    ValueChangedHandler vc = ValueChanged;
                    if (vc != null) { vc(this, new System.EventArgs()); }
                    this.DoInvalidate(); 
                }

                delegate void StartTimerCB();
                private void StartTimer()
                {
                    if (this.InvokeRequired == true)
                    {
                        this.Invoke(new StartTimerCB(StartTimer));
                    }
                    if (mGlowAnimation.Enabled == true)
                        return;
//                    Trace.WriteLine("StartTimer");
                    mGlowAnimation.Start();
                }

                delegate void StopTimerCB();
                private void StopTimer()
                {
                    if (this.InvokeRequired == true)
                    {
                        this.Invoke(new StopTimerCB(StopTimer));
                    }
                    if (mGlowAnimation.Enabled == false)
                        return;
//                    Trace.WriteLine("StopTimer");
                    mGlowAnimation.Stop();
                    mGlowPosition = -320;
                }
								
				private int mMaxValue = 100;
				/// <summary>
				/// The maximum value for the Value property.
				/// </summary>
				[Category("Value"), 
				DefaultValue(100), 
				Description("The maximum value for the Value property.")]
				public int MaxValue
				{
					get { return mMaxValue; }
					set 
					{ 
						mMaxValue = value; 
						if (value > MaxValue) {Value = MaxValue;}
                        if (value > MinValue && value < MaxValue) { StartTimer(); }
                        else { StopTimer(); }
                        MaxChangedHandler mc = MaxChanged;
						if (mc != null){mc(this, new System.EventArgs());}
                        this.DoInvalidate(); 
					}
				}

				private int mMinValue = 0;
				/// <summary>
				/// The minimum value for the Value property.
				/// </summary>
				[Category("Value"), 
				DefaultValue(0), 
				Description("The minimum value for the Value property.")]
				public int MinValue
				{
					get { return mMinValue; }
					set 
					{
						mMinValue = value; 
						if (value < MinValue) {Value = MinValue;}
                        if (value > MinValue && value < MaxValue) { StartTimer(); }
                        else { StopTimer(); }
                        MinChangedHandler mc = MinChanged;
						if (mc != null){mc(this, new System.EventArgs());}
                        this.DoInvalidate(); 
					}
				}

			#endregion
		
			#region -  Bar  -

				private Color mStartColor = Color.FromArgb(210, 0, 0);
				/// <summary>
				/// The start color for the progress bar.
				/// 210, 000, 000 = Red
				/// 210, 202, 000 = Yellow
				/// 000, 163, 211 = Blue
				/// 000, 211, 040 = Green
				/// </summary>
				[Category("Bar"), 
				DefaultValue(typeof(Color), "210, 0, 0"),
				Description("The start color for the progress bar." + 
							"210, 000, 000 = Red\n" + 
							"210, 202, 000 = Yellow\n" + 
							"000, 163, 211 = Blue\n" + 
							"000, 211, 040 = Green\n")]
				public Color StartColor
				{
					get { return mStartColor; }
                    set { mStartColor = value; this.DoInvalidate(); }
				}

				private Color mEndColor = Color.FromArgb(0, 211, 40);
				/// <summary>
				/// The end color for the progress bar.
				/// 210, 000, 000 = Red
				/// 210, 202, 000 = Yellow
				/// 000, 163, 211 = Blue
				/// 000, 211, 040 = Green
				/// </summary>
				[Category("Bar"), 
				DefaultValue(typeof(Color), "0, 211, 40"),
				Description("The end color for the progress bar." + 
					"210, 000, 000 = Red\n" + 
					"210, 202, 000 = Yellow\n" + 
					"000, 163, 211 = Blue\n" + 
					"000, 211, 040 = Green\n")]
				public Color EndColor
				{
					get { return mEndColor; }
                    set { mEndColor = value; this.DoInvalidate(); }
				}

			#endregion

			#region -  Highlights and Glows  -

				private Color mHighlightColor = Color.White;
				/// <summary>
				/// The color of the highlights.
				/// </summary>
				[Category("Highlights and Glows"),
				DefaultValue(typeof(Color),"White"),
				Description("The color of the highlights.")]
				public Color HighlightColor
				{
					get { return mHighlightColor; }
                    set { mHighlightColor = value; this.DoInvalidate(); }
				}

				private Color mBackgroundColor = Color.FromArgb(201,201,201);
				/// <summary>
				/// The color of the background.
				/// </summary>
				[Category("Highlights and Glows"),
				DefaultValue(typeof(Color),"201,201,201"),
				Description("The color of the background.")]
				public Color BackgroundColor
				{
					get { return mBackgroundColor; }
                    set { mBackgroundColor = value; this.DoInvalidate(); }
				}

				private bool mAnimate = true;
				/// <summary>
				/// Whether the glow is animated.
				/// </summary>
				[Category("Highlights and Glows"),
				DefaultValue(typeof(bool), "true"),
				Description("Whether the glow is animated or not.")]
				public bool Animate
				{
					get { return mAnimate; }
					set {
							mAnimate = value; 
							if (value) {StartTimer();} else {StopTimer();}
                            this.DoInvalidate(); 
						}
				}

				private Color mGlowColor = Color.FromArgb(150, 255, 255, 255);
				/// <summary>
				/// The color of the glow.
				/// </summary>
				[Category("Highlights and Glows"),
				DefaultValue(typeof(Color),"150, 255, 255, 255"),
				Description("The color of the glow.")]
				public Color GlowColor
				{
					get { return mGlowColor; }
                    set { mGlowColor = value; this.DoInvalidate(); }
				}
				
			#endregion

		#endregion

		#region -  Drawing  -

            delegate void InvalidateCB();
            public void DoInvalidate()
            {
                if (this.InvokeRequired == true)
                {
                    this.Invoke(new InvalidateCB(DoInvalidate), new object[] { });
                    return;
                }
                this.Invalidate();
            }

			private void DrawBackground(Graphics g)
			{
				Rectangle r = this.ClientRectangle; r.Width--; r.Height--;
				GraphicsPath rr = RoundRect(r, 2, 2, 2, 2);
				g.FillPath(new SolidBrush(this.BackgroundColor), rr);
			}

			private void DrawBackgroundShadows(Graphics g)
			{
				Rectangle lr = new Rectangle(2, 2, 10, this.Height - 5);
				LinearGradientBrush lg = new LinearGradientBrush(lr, Color.FromArgb(30, 0, 0, 0), 
																 Color.Transparent, 
																 LinearGradientMode.Horizontal);
				lr.X--;
				g.FillRectangle(lg, lr);

				Rectangle rr = new Rectangle(this.Width - 12, 2, 10, this.Height - 5);
				LinearGradientBrush rg = new LinearGradientBrush(rr, Color.Transparent, 
																 Color.FromArgb(20, 0, 0, 0),
																 LinearGradientMode.Horizontal);
				g.FillRectangle(rg, rr);
			}

			private void DrawBar(Graphics g)
			{
                if (MaxValue == 0)
                    return;

				Rectangle r = new Rectangle(1, 2, this.Width - 3, this.Height - 3); 
				r.Width = (int)(Value * 1.0F / (MaxValue - MinValue) * this.Width);
				g.FillRectangle(new SolidBrush(GetIntermediateColor()), r);
			}

			private void DrawBarShadows(Graphics g)
			{
                if (MaxValue == 0)
                    return;
                
                Rectangle lr = new Rectangle(1, 2, 15, this.Height - 3);
				LinearGradientBrush lg = new LinearGradientBrush(lr, Color.White, Color.White, 
																 LinearGradientMode.Horizontal);
						
				ColorBlend lc = new ColorBlend(3);
				lc.Colors = new Color[] {Color.Transparent, Color.FromArgb(40, 0, 0, 0), Color.Transparent};
				lc.Positions = new float[] {0.0F, 0.2F, 1.0F};
				lg.InterpolationColors = lc;

				lr.X--;
				g.FillRectangle(lg, lr);

				Rectangle rr = new Rectangle(this.Width - 3, 2, 15, this.Height - 3); 
				rr.X = (int)(Value * 1.0F / (MaxValue - MinValue) * this.Width) - 14;
				LinearGradientBrush rg = new LinearGradientBrush(rr, Color.Black,Color.Black, 
																 LinearGradientMode.Horizontal);

				ColorBlend rc = new ColorBlend(3);
				rc.Colors = new Color[] {Color.Transparent, Color.FromArgb(40, 0, 0, 0), Color.Transparent};
				rc.Positions = new float[] {0.0F, 0.8F, 1.0F};
				rg.InterpolationColors = rc;

				g.FillRectangle(rg, rr);
			}

			private void DrawHighlight(Graphics g)
			{
				Rectangle tr = new Rectangle(1, 1, this.Width - 1, 6);
				GraphicsPath tp = RoundRect(tr, 2, 2, 0, 0);
				
				g.SetClip(tp);
				LinearGradientBrush tg = new LinearGradientBrush(tr, Color.White, 
																 Color.FromArgb(128, Color.White), 
																 LinearGradientMode.Vertical);
				g.FillPath(tg, tp);
				g.ResetClip();

				Rectangle br = new Rectangle(1, this.Height - 8, this.Width - 1, 6);
				GraphicsPath bp = RoundRect(br, 0, 0, 2, 2);
				
				g.SetClip(bp);
				LinearGradientBrush bg = new LinearGradientBrush(br, Color.Transparent, 
																 Color.FromArgb(100, this.HighlightColor),
														  		 LinearGradientMode.Vertical);
				g.FillPath(bg, bp);
				g.ResetClip();
			}

			private void DrawInnerStroke(Graphics g)
			{
				Rectangle r = this.ClientRectangle; 
				r.X++; r.Y++; r.Width-=3; r.Height-=3;
				GraphicsPath rr = RoundRect(r, 2, 2, 2, 2);
				g.DrawPath(new Pen(Color.FromArgb(100, Color.White)), rr);
			}

			private void DrawGlow(Graphics g)
			{
                if (MaxValue == 0)
                    return;
                
                Rectangle r = new Rectangle(mGlowPosition, 0, 60, this.Height);
				LinearGradientBrush lgb = new LinearGradientBrush(r, Color.White, Color.White, 
																  LinearGradientMode.Horizontal);

				ColorBlend cb = new ColorBlend(4);
				cb.Colors = new Color[] {Color.Transparent, this.GlowColor, this.GlowColor, Color.Transparent};
				cb.Positions = new float[] {0.0F, 0.5F, 0.6F, 1.0F};
				lgb.InterpolationColors = cb;

				Rectangle clip = new Rectangle(1, 2, this.Width - 3, this.Height - 3); 
				clip.Width = (int)(Value * 1.0F / (MaxValue - MinValue) * this.Width);
				g.SetClip(clip);
				g.FillRectangle(lgb,r);
				g.ResetClip();
			}

			private void DrawOuterStroke(Graphics g)
			{
				Rectangle r = this.ClientRectangle; r.Width--; r.Height--;
				GraphicsPath rr = RoundRect(r, 2, 2, 2, 2);
				g.DrawPath(new Pen(Color.FromArgb(178, 178, 178)), rr);
			}

            private void DrawText(Graphics g)
            {
                if (MaxValue == 0)
                    return;

                string strText = string.Format("{0}%({1}/{2})", Value * 100 / MaxValue, Value, MaxValue);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                Rectangle r = new Rectangle(0, 2, this.Width, this.Height - 3);
                r.Width = (int)(Value * 1.0F / (MaxValue - MinValue) * this.Width);

                g.SetClip(r);
                g.DrawString(strText, this.Font, new SolidBrush(this.BackColor), new RectangleF(0, 1, this.Width, this.Height), format);
                r.X = r.Width;
                r.Width = this.Width;
                g.SetClip(r);
                g.DrawString(strText, this.Font, new SolidBrush(this.ForeColor), new RectangleF(0, 1, this.Width, this.Height), format);
                g.ResetClip();
            }

		#endregion

		#region -  Functions  -

			private GraphicsPath RoundRect(RectangleF r, float r1, float r2, float r3, float r4)
			{
				float x = r.X, y = r.Y, w = r.Width, h = r.Height;
				GraphicsPath rr = new GraphicsPath();
				rr.AddBezier(x, y + r1, x, y, x + r1, y, x + r1, y);
				rr.AddLine(x + r1, y, x + w - r2, y);
				rr.AddBezier(x + w - r2, y, x + w, y, x + w, y + r2, x + w, y + r2);
				rr.AddLine(x + w, y + r2, x + w, y + h - r3);
				rr.AddBezier(x + w, y + h - r3, x + w, y + h, x + w - r3, y + h, x + w - r3, y + h);
				rr.AddLine(x + w - r3, y + h, x + r4, y + h);
				rr.AddBezier(x + r4, y + h, x, y + h, x, y + h - r4, x, y + h - r4);
				rr.AddLine(x, y + h - r4, x, y + r1);
				return rr;
			}

			private bool InDesignMode()
			{
				return (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}

			private Color GetIntermediateColor()
			{
				Color c = this.StartColor;
				Color c2 = this.EndColor;

				float pc = this.Value * 1.0F / (this.MaxValue - this.MinValue);

				int ca = c.A, cr = c.R, cg = c.G, cb = c.B;
				int c2a = c2.A, c2r = c2.R, c2g = c2.G, c2b = c2.B;
				
				int a = (int)Math.Abs(ca + (ca - c2a) * pc);
				int r = (int)Math.Abs(cr - ((cr - c2r) * pc));
				int g = (int)Math.Abs(cg - ((cg - c2g) * pc));
				int b = (int)Math.Abs(cb - ((cb - c2b) * pc));

				if (a > 255) {a = 255;}
				if (r > 255) {r = 255;}
				if (g > 255) {g = 255;}
				if (b > 255) {b = 255;}

				return (Color.FromArgb(a, r, g, b));
			}

		#endregion

		#region -  Other  -

            delegate void PaintCB(object sender, PaintEventArgs e);
            private void ProgressBar_Paint(object sender, PaintEventArgs e)
			{
                if (this.InvokeRequired == true)
                {
                    this.Invoke(new PaintCB(ProgressBar_Paint), new object[] { sender, e });
                    return;
                }
                
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				DrawBackground(e.Graphics);
				DrawBackgroundShadows(e.Graphics);
				DrawBar(e.Graphics);
				DrawBarShadows(e.Graphics);
				DrawHighlight(e.Graphics);
				DrawInnerStroke(e.Graphics);
				DrawGlow(e.Graphics);
				DrawOuterStroke(e.Graphics);
                DrawText(e.Graphics);
			}

			private void mGlowAnimation_Tick(object sender, EventArgs e)
			{
//                Trace.WriteLine("mGlowAnimation_Tick");
				if (this.Animate)
				{
					mGlowPosition += 4;
					if (mGlowPosition > this.Width)
					{
						mGlowPosition = -300;
					}
					this.DoInvalidate();
				}
				else
				{
                    StopTimer();
				}
			}

		#endregion

		#region -  Events  -

			/// <summary>
			/// When the Value property is changed.
			/// </summary>
			public delegate void ValueChangedHandler(object sender, EventArgs e);
			/// <summary>
			/// When the Value property is changed.
			/// </summary>
			public event ValueChangedHandler ValueChanged;

			/// <summary>
			/// When the MinValue property is changed.
			/// </summary>
			public delegate void MinChangedHandler(object sender, EventArgs e);
			/// <summary>
			/// When the MinValue property is changed.
			/// </summary>
			public event MinChangedHandler MinChanged;
				
			/// <summary>
			/// When the MaxValue property is changed.
			/// </summary>
			public delegate void MaxChangedHandler(object sender, EventArgs e);
			/// <summary>
			/// When the MaxValue property is changed.
			/// </summary>
			public event MaxChangedHandler MaxChanged;

		#endregion

	}
}