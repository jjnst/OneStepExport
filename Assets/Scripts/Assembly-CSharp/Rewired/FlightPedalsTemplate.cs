using System;

namespace Rewired
{
	public sealed class FlightPedalsTemplate : ControllerTemplate, IFlightPedalsTemplate, IControllerTemplate
	{
		public static readonly Guid typeGuid = new Guid("f6fe76f8-be2a-4db2-b853-9e3652075913");

		public const int elementId_leftPedal = 0;

		public const int elementId_rightPedal = 1;

		public const int elementId_slide = 2;

		IControllerTemplateAxis IFlightPedalsTemplate.leftPedal
		{
			get
			{
				return GetElement<IControllerTemplateAxis>(0);
			}
		}

		IControllerTemplateAxis IFlightPedalsTemplate.rightPedal
		{
			get
			{
				return GetElement<IControllerTemplateAxis>(1);
			}
		}

		IControllerTemplateAxis IFlightPedalsTemplate.slide
		{
			get
			{
				return GetElement<IControllerTemplateAxis>(2);
			}
		}

		public FlightPedalsTemplate(object payload)
			: base(payload)
		{
		}
	}
}
