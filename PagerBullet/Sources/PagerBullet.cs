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

		public PagerBullet(Context context, AttributeSet attrs) : base(context, attrs)
		{
			Init(context);
			SetAttributes(context, attrs);
		}

		public PagerBullet(Context context, AttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Init(context);
			SetAttributes(context, attrs);
		}

		private void SetAttributes(Context context, AttributeSet attrs)
		{
			TypedArray typedArray = context.ObtainStyledAttributes(attrs, R.styleable.PagerBullet);
			string heightValue = typedArray.GetString(R.styleable.PagerBullet_panelHeightInDp);

			if (null != heightValue)
			{
				heightValue = heightValue.ReplaceAll(DIGIT_PATTERN, "");
				float height = Float.ParseFloat(heightValue);
				FrameLayout.LayoutParams params = (LayoutParams)indicatorContainer.LayoutParams;
	            params.height = Math.Round(TypedValue.ApplyDimension(TypedValue.COMPLEX_UNIT_DIP, height,
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

		public ViewPager getViewPager()
		{
			return viewPager;
		}

		public void addOnPageChangeListener(ViewPager.OnPageChangeListener onPageChangeListener)
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
			const bool hasSeparator = HasSeparator();
			textIndicator.Visibility = hasSeparator ? VISIBLE : INVISIBLE;
			layoutIndicator.Visibility = hasSeparator ? INVISIBLE : VISIBLE;

			if (!hasSeparator)
			{
				InitIndicator(adapter.Count);
			}

			SetIndicatorItem(viewPager.CurrentItem);
		}

		private void Init(Context context)
		{
			LayoutInflater layoutInflater = LayoutInflater.From(context);
			View rootView = layoutInflater.Inflate(R.layout.item_view_pager, this);
			indicatorContainer = rootView.FindViewById(R.id.pagerBulletIndicatorContainer);
			textIndicator = (TextView)indicatorContainer.FindViewById(R.id.pagerBulletIndicatorText);
			layoutIndicator = (LinearLayout)indicatorContainer.FindViewById(R.id.pagerBulletIndicator);

			viewPager = (ViewPager)rootView.FindViewById(R.id.viewPagerBullet);
			viewPager.addOnPageChangeListener(new ViewPager.OnPageChangeListener() {

				@Override
				public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels)
				{

				}

				@Override
				public void onPageSelected(int position)
				{
					SetIndicatorItem(position);
				}

				@Override
				public void onPageScrollStateChanged(int state)
				{

				}
			});
    	}

	    private void InitIndicator(int count)
		{
			layoutIndicator.RemoveAllViews();
			LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(
				LinearLayout.LayoutParams.WrapContent,
				LinearLayout.LayoutParams.WrapContent
			);

			int margin = Math.Round(Resources
					.GetDimension(R.dimen.pager_bullet_indicator_dot_margin));

        	params.setMargins(margin, 0, margin, 0);
			Drawable drawableInactive = ContextCompat.GetDrawable(Context,
					R.drawable.inactive_dot);

			for (int i = 0; i < count; i++)
			{
				ImageView imageView = new ImageView(Context);
				imageView.SetImageDrawable(drawableInactive);
				layoutIndicator.AddView(imageView, params);
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
			PagerAdapter adapter = viewPager.getAdapter();
			if (null != adapter)
			{
				const int count = adapter.Count;
				textIndicator.setText(String.format(getContext()
						.getString(R.string.pager_bullet_separator), index + 1, count));
			}
		}

		private void SetItemBullet(int selectedPosition)
		{
			Drawable drawableInactive = ContextCompat.GetDrawable(Context, R.drawable.inactive_dot);
			drawableInactive = WrapTintDrawable(drawableInactive, inactiveColorTint);
			Drawable drawableActive = ContextCompat.GetDrawable(getContext(), R.drawable.active_dot);
			drawableActive = WrapTintDrawable(drawableActive, activeColorTint);

			const int indicatorItemsCount = layoutIndicator.getChildCount();

			for (int position = 0; position < indicatorItemsCount; position++)
			{
				ImageView imageView = (ImageView)layoutIndicator.getChildAt(position);

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