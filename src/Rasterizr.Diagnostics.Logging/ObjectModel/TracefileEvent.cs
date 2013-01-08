using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rasterizr.Diagnostics.Logging.ObjectModel
{
	public class TracefileEvent
	{
		public int Number { get; set; }
		public OperationType OperationType { get; set; }
		public TracefileEventArgumentCollection Arguments { get; set; }

		public List<PixelEvent> PixelEvents { get; set; }

		public TracefileEvent()
		{
			Arguments = new TracefileEventArgumentCollection();
			PixelEvents = new List<PixelEvent>();
		}
	}
}