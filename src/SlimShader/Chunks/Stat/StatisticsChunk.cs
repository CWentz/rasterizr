using System.Text;
using SlimShader.IO;
using SlimShader.Shader;

namespace SlimShader.Chunks.Stat
{
	/// <summary>
	/// Statistics chunk. Thanks to Wine for the hard work in decoding this.
	/// http://source.winehq.org/source/dlls/d3dcompiler_43/reflection.c#L1061
	/// Based on D3D11_SHADER_DESC.
	/// </summary>
	public class StatisticsChunk : DxbcChunk
	{
		/// <summary>
		/// The number of intermediate-language instructions in the compiled shader.
		/// </summary>
		public uint InstructionCount { get; private set; }

		/// <summary>
		/// The number of temporary registers in the compiled shader.
		/// </summary>
		public uint TempRegisterCount { get; private set; }

		/// <summary>
		/// Number of temporary arrays used.
		/// </summary>
		public uint TempArrayCount { get; private set; }

		/// <summary>
		/// Number of constant defines.
		/// </summary>
		public uint DefineCount { get; private set; }

		/// <summary>
		/// Number of declarations (input + output).
		/// </summary>
		public uint DeclarationCount { get; private set; }

		/// <summary>
		/// Number of non-categorized texture instructions.
		/// </summary>
		public uint TextureNormalInstructions { get; private set; }

		/// <summary>
		/// Number of texture load instructions
		/// </summary>
		public uint TextureLoadInstructions { get; private set; }

		/// <summary>
		/// Number of texture comparison instructions
		/// </summary>
		public uint TextureCompInstructions { get; private set; }

		/// <summary>
		/// Number of texture bias instructions
		/// </summary>
		public uint TextureBiasInstructions { get; private set; }

		/// <summary>
		/// Number of texture gradient instructions.
		/// </summary>
		public uint TextureGradientInstructions { get; private set; }

		/// <summary>
		/// Number of floating point arithmetic instructions used.
		/// </summary>
		public uint FloatInstructionCount { get; private set; }

		/// <summary>
		/// Number of signed integer arithmetic instructions used.
		/// </summary>
		public uint IntInstructionCount { get; private set; }

		/// <summary>
		/// Number of unsigned integer arithmetic instructions used.
		/// </summary>
		public uint UIntInstructionCount { get; private set; }

		/// <summary>
		/// Number of static flow control instructions used.
		/// </summary>
		public uint StaticFlowControlCount { get; private set; }

		/// <summary>
		/// Number of dynamic flow control instructions used.
		/// </summary>
		public uint DynamicFlowControlCount { get; private set; }

		/// <summary>
		/// Number of macro instructions used.
		/// </summary>
		public uint MacroInstructionCount { get; private set; }

		/// <summary>
		/// Number of array instructions used.
		/// </summary>
		public uint ArrayInstructionCount { get; private set; }

		/// <summary>
		/// Number of cut instructions used.
		/// </summary>
		public uint CutInstructionCount { get; private set; }

		/// <summary>
		/// Number of emit instructions used.
		/// </summary>
		public uint EmitInstructionCount { get; private set; }

		/// <summary>
		/// The <see cref="PrimitiveTopology" />-typed value that represents the geometry shader output topology.
		/// </summary>
		public PrimitiveTopology GeometryShaderOutputTopology { get; private set; }

		/// <summary>
		/// Geometry shader maximum output vertex count.
		/// </summary>
		public uint GeometryShaderMaxOutputVertexCount { get; private set; }

		/// <summary>
		/// The <see cref="Primitive"/>-typed value that represents the input primitive for a geometry shader or hull shader.
		/// </summary>
		public Primitive InputPrimitive { get; private set; }

		/// <summary>
		/// Number of geometry shader instances.
		/// </summary>
		public uint GeometryShaderInstanceCount { get; private set; }

		/// <summary>
		/// Number of control points in the hull shader and domain shader.
		/// </summary>
		public uint ControlPoints { get; private set; }

		/// <summary>
		/// The <see cref="TessellatorOutputPrimitive"/>-typed value that represents the tessellator output-primitive type.
		/// </summary>
		public TessellatorOutputPrimitive HullShaderOutputPrimitive { get; private set; }

		/// <summary>
		/// The <see cref="TessellatorPartitioning" />-typed value that represents the tessellator partitioning mode.
		/// </summary>
		public TessellatorPartitioning HullShaderPartitioning { get; private set; }

		/// <summary>
		/// The <see cref="TessellatorDomain"/>-typed value that represents the tessellator domain.
		/// </summary>
		public TessellatorDomain TessellatorDomain { get; private set; }

		/// <summary>
		/// Number of barrier instructions in a compute shader.
		/// </summary>
		public uint BarrierInstructions { get; private set; }

		/// <summary>
		/// Number of interlocked instructions in a compute shader.
		/// </summary>
		public uint InterlockedInstructions { get; private set; }

		/// <summary>
		/// Number of texture writes in a compute shader.
		/// </summary>
		public uint TextureStoreInstructions { get; private set; }

		public uint MovInstructionCount { get; private set; }
		public uint MovCInstructionCount { get; private set; }
		public uint Unknown3 { get; private set; }
		public uint ConversionInstructionCount { get; private set; }
		public uint Unknown4 { get; private set; }
		public uint Unknown5 { get; private set; }
		public uint Unknown6 { get; private set; }
		public uint Unknown7 { get; private set; }

		public static StatisticsChunk Parse(BytecodeReader reader, uint chunkSize)
		{
			var size = chunkSize / sizeof(uint);

			// Unknowns:
			// DefCount
			// MacroInstructionCount

			// PatchConstantParameters
			// cGSInstanceCount
			// cBarrierInstructions
			// cInterlockedInstructions
			// cTextureStoreInstructions

			var result = new StatisticsChunk
			{
				InstructionCount = reader.ReadUInt32(),
				TempRegisterCount = reader.ReadUInt32(),
				DefineCount = reader.ReadUInt32(), // Guessed
				DeclarationCount = reader.ReadUInt32(),
				FloatInstructionCount = reader.ReadUInt32(),
				IntInstructionCount = reader.ReadUInt32(),
				UIntInstructionCount = reader.ReadUInt32(),
				StaticFlowControlCount = reader.ReadUInt32(),
				DynamicFlowControlCount = reader.ReadUInt32(),
				MacroInstructionCount = reader.ReadUInt32(), // Guessed
				TempArrayCount = reader.ReadUInt32(),
				ArrayInstructionCount = reader.ReadUInt32(),
				CutInstructionCount = reader.ReadUInt32(),
				EmitInstructionCount = reader.ReadUInt32(),
				TextureNormalInstructions = reader.ReadUInt32(),
				TextureLoadInstructions = reader.ReadUInt32(),
				TextureCompInstructions = reader.ReadUInt32(),
				TextureBiasInstructions = reader.ReadUInt32(),
				TextureGradientInstructions = reader.ReadUInt32(),
				MovInstructionCount = reader.ReadUInt32(),
				MovCInstructionCount = reader.ReadUInt32(), // Guessed
				ConversionInstructionCount = reader.ReadUInt32(),
				Unknown4 = reader.ReadUInt32(),
				InputPrimitive = (Primitive) reader.ReadUInt32(),
				GeometryShaderOutputTopology = (PrimitiveTopology) reader.ReadUInt32(),
				GeometryShaderMaxOutputVertexCount = reader.ReadUInt32(),
				Unknown5 = reader.ReadUInt32(),
				Unknown6 = reader.ReadUInt32(),
				Unknown7 = reader.ReadUInt32(),
			};

			// DX10 stat size
			if (size == 29)
				return result;

			// Unknown.
			var unknown = reader.ReadUInt32();

			result.ControlPoints = reader.ReadUInt32();
			result.HullShaderOutputPrimitive = (TessellatorOutputPrimitive) reader.ReadUInt32();
			result.HullShaderPartitioning = (TessellatorPartitioning) reader.ReadUInt32();
			result.TessellatorDomain = (TessellatorDomain) reader.ReadUInt32();

			result.BarrierInstructions = reader.ReadUInt32(); // Guessed
			result.InterlockedInstructions = reader.ReadUInt32(); // Guessed
			result.TextureStoreInstructions = reader.ReadUInt32(); // Guessed

			// DX11 stat size.
			if (size == 37)
				return result;

			throw new ParseException("Unhandled stat size: " + chunkSize);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (TessellatorDomain != TessellatorDomain.Undefined)
			{
				sb.AppendLine("// Tessellation Domain   # of control points");
				sb.AppendLine("// -------------------- --------------------");
				sb.AppendLine(string.Format("// {0,-20} {1,20}", TessellatorDomain.GetDescription(), ControlPoints));
				sb.AppendLine("//");
			}
			if (HullShaderOutputPrimitive != TessellatorOutputPrimitive.Undefined)
			{
				sb.AppendLine("// Tessellation Output Primitive  Partitioning Type ");
				sb.AppendLine("// ------------------------------ ------------------");
				sb.AppendLine(string.Format("// {0,-30} {1,-18}", HullShaderOutputPrimitive.GetDescription(),
					HullShaderPartitioning.GetDescription()));
				sb.AppendLine("//");
			}
			return sb.ToString();
		}
	}
}