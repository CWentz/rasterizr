﻿using System.ComponentModel;

namespace SlimShader.Shader
{
	public enum InstructionTestBoolean
	{
		[Description("z")]
		Zero = 0,

		[Description("nz")]
		NonZero = 1
	}
}