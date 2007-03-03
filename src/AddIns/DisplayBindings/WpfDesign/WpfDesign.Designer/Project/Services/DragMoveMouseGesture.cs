﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ICSharpCode.WpfDesign.Designer.Services
{
	/// <summary>
	/// Mouse gesture for moving elements inside a container or between containers.
	/// Belongs to the PointerTool.
	/// </summary>
	sealed class DragMoveMouseGesture : ClickOrDragMouseGesture
	{
		DesignItem clickedOn;
		PlacementOperation operation;
		
		internal DragMoveMouseGesture(DesignItem clickedOn)
		{
			Debug.Assert(clickedOn != null);
			
			this.clickedOn = clickedOn;
			
			if (clickedOn.Parent != null)
				this.positionRelativeTo = clickedOn.Parent.View;
			else
				this.positionRelativeTo = clickedOn.View;
		}
		
		double startLeft, startRight, startTop, startBottom;
		
		protected override void OnDragStarted()
		{
			IPlacementBehavior b = PlacementOperation.GetPlacementBehavior(clickedOn);
			if (b != null && b.CanPlace(clickedOn, PlacementType.Move, PlacementAlignments.TopLeft)) {
				operation = PlacementOperation.Start(clickedOn, PlacementType.Move);
				startLeft = operation.Left;
				startRight = operation.Right;
				startTop = operation.Top;
				startBottom = operation.Bottom;
			}
		}
		
		protected override void OnMouseMove(object sender, MouseEventArgs e)
		{
			base.OnMouseMove(sender, e); // call OnDragStarted if min. drag distace is reached
			if (operation != null) {
				UIElement currentContainer = operation.CurrentContainer.View;
				Point p = e.GetPosition(currentContainer);
				if (p.X < 0 || p.Y < 0 || p.X > currentContainer.RenderSize.Width || p.Y > currentContainer.RenderSize.Height) {
					// outside the bounds of the current container
					if (operation.CurrentContainerBehavior.CanLeaveContainer(operation)) {
						if (ChangeContainerIfPossible(e)) {
							return;
						}
					}
				}
				
				Vector v = e.GetPosition(positionRelativeTo) - startPoint;
				operation.Left = startLeft + v.X;
				operation.Right = startRight + v.X;
				operation.Top = startTop + v.Y;
				operation.Bottom = startBottom + v.Y;
				operation.CurrentContainerBehavior.UpdatePlacement(operation);
			}
		}
		
		// Perform hit testing on the design panel and return the first model that is not selected
		DesignPanelHitTestResult HitTestUnselectedModel(MouseEventArgs e)
		{
			DesignPanelHitTestResult result = DesignPanelHitTestResult.NoHit;
			ISelectionService selection = services.Selection;
			designPanel.HitTest(
				e, false, true,
				delegate(DesignPanelHitTestResult r) {
					if (r.ModelHit == null)
						return true; // continue hit testing
					if (selection.IsComponentSelected(r.ModelHit))
						return true; // continue hit testing
					result = r;
					return false; // finish hit testing
				});
			return result;
		}
		
		bool ChangeContainerIfPossible(MouseEventArgs e)
		{
			DesignPanelHitTestResult result = HitTestUnselectedModel(e);
			if (result.ModelHit == null) return false;
			
			// check that we don't move an item into itself:
			DesignItem tmp = result.ModelHit;
			while (tmp != null) {
				if (tmp == clickedOn) return false;
				tmp = tmp.Parent;
			}
			
			IPlacementBehavior b = result.ModelHit.GetBehavior<IPlacementBehavior>();
			if (b != null && b.CanEnterContainer(operation)) {
				operation.ChangeContainer(result.ModelHit);
				return true;
			}
			return false;
		}
		
		protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (operation != null) {
				operation.Commit();
				operation = null;
			}
			Stop();
		}
		
		protected override void OnStopped()
		{
			if (operation != null) {
				operation.Abort();
				operation = null;
			}
		}
	}
}
