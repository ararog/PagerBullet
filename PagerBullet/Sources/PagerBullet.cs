using System.Text.RegularExpressions;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PagerBullet;

namespace Com.Robohorse.PagerBullet
{
	/**
	 * Created by vadim on 15.06.16.
	 */
	public class PagerBullet : FrameLayout
	{

		private const string DIGIT_PATTERN = "[^0-9.]";
	    private const int DEFAULT_INDICATOR_OFFSET_VALUE = 20;
		private int offset = DEFAULT_INDICATOR_OFFSET_VALUE;

		private ViewPager viewPager;
		private TextView textIndicator;
		private LinearLayout layoutIndicator;
		private View indicatorContainer;

		private int activeColorTint;
		private int inactiveColorTint;

		public PagerBullet(Context context) : base(context)
		{
			Init(context);
		}

		public PagerBullet(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init(context);
			SetAttributes(context, attrs);
		}

		public PagerBullet(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Init(context);
			SetAttributes(context, attrs);
		}

		private void SetAttributes(Context context, IAttributeSet attrs)
		{
			TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.PagerBullet);
			string heightValue = typedArray.GetString(Resource.Styleable.PagerBullet_panelHeightInDp);

			if (null != heightValue)
			{
				Regex rgx = new Regex(DIGIT_PATTERN);
				heightValue = rgx.Replace(heightValue, "");
				float height = Float.ParseFloat(heightValue);
				FrameLayout.LayoutParams parameters = (LayoutParams)indicatorContainer.LayoutParameters;
				parameters.Height = Math.Round(TypedValue.ApplyDimension(ComplexUnitType.Dip, height,
						Resources.DisplayMetrics));
				indicatorContainer.RequestLayout();
			}
			typedArray.Recycle();
		}

		public void SetIndicatorTintColorScheme(int activeColorTint, int inactiveColorTint)
		{
			this.activeColorTint = activeColorTint;
			this.inactiveColorTint = inactiveColorTint;
			invalidateBullets();
		}

		public void SetTextSeparatorOffset(int offset)
		{
			this.offset = offset;
		}

		public void SetAdapter(PagerAdapter adapter)
		{
			viewPager.Adapter = adapter;
			InvalidateBullets(adapter);
		}

		public void setCurrentItem(int position)
		{
			viewPager.CurrentItem = position;
			SetIndicatorItem(position);
		}

		public ViewPager ViewPager
		{
			get
			{
				return viewPager;
			}
		}

		public void AddOnPageChangeListener(ViewPager.IOnPageChangeListener onPageChangeListener)
		{
			viewPager.AddOnPageChangeListener(onPageChangeListener);
		}

		public void invalidateBullets()
		{
			PagerAdapter adapter = viewPager.Adapter;
			if (null != adapter)
			{
				InvalidateBullets(adapter);
			}
		}

		public void InvalidateBullets(PagerAdapter adapter)
		{
			bool hasSeparator = HasSeparator();
			textIndicator.Visibility = hasSeparator ? ViewStates.Visible : ViewStates.Invisible;
			layoutIndicator.Visibility = hasSeparator ? ViewStates.Invisible : ViewStates.Visible;

			if (!hasSeparator)
			{
				InitIndicator(adapter.Count);
			}

			SetIndicatorItem(viewPager.CurrentItem);
		}

		private void Init(Context context)
		{
			LayoutInflater layoutInflater = LayoutInflater.From(context);
			View rootView = layoutInflater.Inflate(Resource.Layout.item_view_pager, this);
			indicatorContainer = rootView.FindViewById(Resource.Id.pagerBulletIndicatorContainer);
			textIndicator = (TextView)indicatorContainer.FindViewById(Resource.Id.pagerBulletIndicatorText);
			layoutIndicator = (LinearLayout)indicatorContainer.FindViewById(Resource.Id.pagerBulletIndicator);

			viewPager = (ViewPager)rootView.FindViewById(Resource.Id.viewPagerBullet);
			viewPager.PageSelected += (sender, e) =>
			{
				SetIndicatorItem(e.Position);
			};
    	}

	    private void InitIndicator(int count)
		{
			layoutIndicator.RemoveAllViews();
			LinearLayout.LayoutParams parameters = new LinearLayout.LayoutParams(
				ViewGroup.LayoutParams.WrapContent,
				ViewGroup.LayoutParams.WrapContent
			);

			int margin = Math.Round(Resources.GetDimension(Resource.Dimension.pager_bullet_indicator_dot_margin));

        	parameters.SetMargins(margin, 0, margin, 0);
			Drawable drawableInactive = ContextCompat.GetDrawable(Context,
					Resource.Drawable.inactive_dot);

			for (int i = 0; i < count; i++)
			{
				ImageView imageView = new ImageView(Context);
				imageView.SetImageDrawable(drawableInactive);
				layoutIndicator.AddView(imageView, parameters);
			}
		}

		private void SetIndicatorItem(int index)
		{
			if (!HasSeparator())
			{
				SetItemBullet(index);
			}
			else {
				SetItemText(index);
			}
		}

		private bool HasSeparator()
		{
			PagerAdapter pagerAdapter = viewPager.Adapter;
			return null != pagerAdapter && pagerAdapter.Count > offset;
		}

		private void SetItemText(int index)
		{
			PagerAdapter adapter = viewPager.Adapter;
			if (null != adapter)
			{
				int count = adapter.Count;
				textIndicator.Text = String.Format(Context
						.GetString(Resource.String.pager_bullet_separator), index + 1, count);
			}
		}

		private void SetItemBullet(int selectedPosition)
		{
			Drawable drawableInactive = ContextCompat.GetDrawable(Context, Resource.Drawable.inactive_dot);
			drawableInactive = WrapTintDrawable(drawableInactive, inactiveColorTint);
			Drawable drawableActive = ContextCompat.GetDrawable(Context, Resource.Drawable.active_dot);
			drawableActive = WrapTintDrawable(drawableActive, activeColorTint);

			int indicatorItemsCount = layoutIndicator.ChildCount;

			for (int position = 0; position < indicatorItemsCount; position++)
			{
				ImageView imageView = (ImageView)layoutIndicator.GetChildAt(position);

				if (position != selectedPosition)
				{
					imageView.SetImageDrawable(drawableInactive);

				}
				else {
					imageView.SetImageDrawable(drawableActive);
				}
			}
		}

		public static Drawable WrapTintDrawable(Drawable sourceDrawable, int color)
		{
			if (color != 0)
			{
				Drawable wrapDrawable = DrawableCompat.Wrap(sourceDrawable);
				DrawableCompat.SetTint(wrapDrawable, color);
				wrapDrawable.SetBounds(0, 0, wrapDrawable.IntrinsicWidth,
						wrapDrawable.IntrinsicHeight);
				return wrapDrawable;

			}
			else {
				return sourceDrawable;
			}
		}
	}
}