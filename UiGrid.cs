using SFML.Graphics;
using SFML.System;

namespace Example
{
	class UiGrid : Transformable, Drawable
	{
		public UiGrid(uint columns, uint rows, Vector2f position, Vector2f size, Color color)
		{
			Position = position;
			Size = size;
			var font = new Font("Content/sansation.ttf");
			var textBlueprint = new Text("", font) { FillColor = color };
			cellTexts = new Text[columns, rows];
			vertices = new VertexArray(PrimitiveType.Lines);
			var deltaX = new Vector2f(size.X / columns, 0f);
			var deltaY = new Vector2f(0f, size.Y / rows);
			for(int column = 0; column < columns; ++column)
			{
				for (int row = 0; row < rows; ++row)
				{
					var text = new Text(textBlueprint);
					text.Position = (0.5f + column) * deltaX + (0.5f + row) * deltaY;
					cellTexts[column, columns - 1 - row] = text;
				}

			}
			// vertical lines
			for (int column = 0; column < columns + 1; ++column)
			{
				var newPosX = column * deltaX;
				vertices.Append(new Vertex(newPosX, color));
				vertices.Append(new Vertex(newPosX + new Vector2f(0f, size.Y), color));
			}
			for (int row = 0; row < rows + 1; ++row)
			{
				var newPosY = row * deltaY;
				vertices.Append(new Vertex(newPosY, color));
				vertices.Append(new Vertex(newPosY + new Vector2f(size.X, 0f), color));
			}

			Columns = columns;
			Rows = rows;
		}

		public string this[int column, int row]
		{
			get => cellTexts[column, row].DisplayedString;
			set
			{
				var text = cellTexts[column, row];
				text.DisplayedString = value;
				var localBounds = text.GetLocalBounds();
				text.Origin = new Vector2f(0.5f * (localBounds.Left + localBounds.Width), 0.5f * (localBounds.Top + localBounds.Height));
			}
		}

		public void Draw(RenderTarget target, RenderStates states)
		{
			// apply the entity's transform -- combine it with the one that was passed by the caller
			states.Transform *= Transform;
			foreach (var cell in cellTexts)
			{
				target.Draw(cell, states);
			}
			// draw the vertex array
			target.Draw(vertices, states);
		}


		public uint Columns { get; }
		public uint Rows { get; }

		public Vector2f Size { get; private set; }

		private readonly Text[,] cellTexts;
		private readonly VertexArray vertices;
	}
}
