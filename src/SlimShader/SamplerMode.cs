﻿using System.ComponentModel;

namespace SlimShader
{
	public enum SamplerMode
	{
		[Description("mode_default")]
		Default = 0,

		[Description("comparison")]
		Comparison = 1,

		[Description("mono")]
		Mono = 2
	}
}