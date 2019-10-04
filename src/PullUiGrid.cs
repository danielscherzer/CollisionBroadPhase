using SFML.Graphics;
using SFML.System;
using System;

namespace Example
{
	class PullUiGrid :  Transformable, Drawable, IRectangleShape
	{
		private readonly UiGrid uiGrid;
		private readonly Func<int, int, string> getCellString;

		public PullUiGrid(uint columns, uint rows, Vector2f position, Vector2f size, Color color, Font font, Func<int, int, string> getCellString) 
		{
			uiGrid = new UiGrid(columns, rows, position, size, color, font);
			this.getCellString = getCellString ?? throw new ArgumentNullException(nameof(getCellString));
		}

		public Vector2f Size => uiGrid.Size;

		public void Draw(RenderTarget target, RenderStates states)
		{
			Update();
			uiGrid.Draw(target, states);
		}

		private void Update()
		{
			for (int column = 0; column < uiGrid.Columns; ++column)
			{
				for (int row = 0; row < uiGrid.Rows; ++row)
				{
					uiGrid[column, row] = getCellString(column, row);
				}
			}
		}
	}
}
