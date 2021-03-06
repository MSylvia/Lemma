﻿using System; using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemma.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Lemma.Components
{
	public class Scroller : UIComponent
	{
		public Property<float> ScrollAmount = new Property<float> { Value = 30.0f };

		public Property<bool> EnableScroll = new Property<bool> { Value = true };

		public Property<bool> DefaultScrollHorizontal = new Property<bool>();

		public Property<bool> ResizeHorizontal = new Property<bool>();

		public Property<float> MaxHorizontalSize = new Property<float>();

		public Property<bool> ResizeVertical = new Property<bool>();

		public Property<float> MaxVerticalSize = new Property<float>();

		private NotifyBinding binding = null;

		public Scroller()
		{
			this.EnableScissor.Value = true;
		}
		
		public override void Awake()
		{
			base.Awake();
			new CommandBinding<int>(this.MouseScrolled, delegate(int delta)
			{
				if (this.Children.Length == 1 && this.EnableScroll)
				{
					UIComponent child = this.Children.First();
					bool horizontalScroll = this.main.KeyboardState.Value.IsKeyDown(Keys.LeftShift);
					if (this.DefaultScrollHorizontal)
						horizontalScroll = !horizontalScroll;
					Vector2 newPosition = child.Position + (horizontalScroll ? new Vector2(delta * this.ScrollAmount, 0) : new Vector2(0, delta * this.ScrollAmount));

					newPosition.X = Math.Max(newPosition.X, this.Size.Value.X - child.ScaledSize.Value.X);
					newPosition.X = Math.Min(newPosition.X, 0);

					newPosition.Y = Math.Max(newPosition.Y, this.Size.Value.Y - child.ScaledSize.Value.Y);
					newPosition.Y = Math.Min(newPosition.Y, 0);
					child.Position.Value = newPosition;
				}
			});
			this.Add(new ListNotifyBinding<UIComponent>(this.childrenChanged, this.Children));
			this.childrenChanged();
		}

		private void childrenChanged()
		{
			this.layoutDirty = true;
			if (this.binding != null)
				this.Remove(this.binding);
			this.binding = null;
		}

		protected override void updateLayout()
		{
			if (this.binding == null)
			{
				if (this.Children.Length == 1)
				{
					UIComponent child = this.Children.First();
					this.binding = new NotifyBinding(delegate() { this.layoutDirty = true; }, this.Size, child.ScaledSize);
					this.Add(this.binding);
				}
			}

			if (this.Children.Length == 1)
			{
				UIComponent child = this.Children.First();
				Vector2 newPosition = child.Position;

				if (this.ResizeHorizontal)
				{
					float size = child.ScaledSize.Value.X;
					if (size > this.MaxHorizontalSize && this.MaxHorizontalSize != 0.0f)
						size = this.MaxHorizontalSize;
					this.Size.Value = new Vector2(size, this.Size.Value.Y);
				}
				newPosition.X = Math.Max(newPosition.X, this.Size.Value.X - child.ScaledSize.Value.X);
				newPosition.X = Math.Min(newPosition.X, 0);

				if (this.ResizeVertical)
				{
					float size = child.ScaledSize.Value.Y;
					if (size > this.MaxVerticalSize && this.MaxVerticalSize != 0.0f)
						size = this.MaxVerticalSize;
					this.Size.Value = new Vector2(this.Size.Value.X, size);
				}
				newPosition.Y = Math.Max(newPosition.Y, this.Size.Value.Y - child.ScaledSize.Value.Y);
				newPosition.Y = Math.Min(newPosition.Y, 0);
				child.Position.Value = newPosition;
			}
		}

		public void ScrollToBottom()
		{
			if (this.Children.Length == 1)
			{
				UIComponent child = this.Children.First();

				Vector2 newPosition = child.Position;
				newPosition.Y = Math.Min(this.Size.Value.Y - child.ScaledSize.Value.Y, 0);
				child.Position.Value = newPosition;
			}
		}

		public void ScrollTo(UIComponent target)
		{
			if (this.Children.Length == 1)
			{
				UIComponent child = this.Children.First();

				Vector2 newPosition = child.Position;

				Vector2 targetPos = target.ScaledSize.Value;
				while (target != child)
				{
					targetPos += target.Position;
					target = target.Parent;
				}

				newPosition.Y = Math.Min(this.Size.Value.Y - targetPos.Y, 0);
				child.Position.Value = newPosition;
			}
		}

		public void ScrollToTop()
		{
			if (this.Children.Length == 1)
			{
				UIComponent child = this.Children.First();

				Vector2 newPosition = child.Position;
				newPosition.Y = Math.Max(this.Size.Value.Y - child.ScaledSize.Value.Y, 0);
				child.Position.Value = newPosition;
			}
		}
	}
}
