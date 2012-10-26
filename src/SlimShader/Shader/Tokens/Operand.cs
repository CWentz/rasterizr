using System;
using System.Linq;
using SlimShader.IO;
using SlimShader.Util;

namespace SlimShader.Shader.Tokens
{
	/// <summary>
	/// Instruction Operand Format (OperandToken0)
	///
	/// [01:00] D3D10_SB_OPERAND_NUM_COMPONENTS
	/// [11:02] Component Selection
	///         if([01:00] == D3D10_SB_OPERAND_0_COMPONENT)
	///              [11:02] = Ignored, 0
	///         else if([01:00] == D3D10_SB_OPERAND_1_COMPONENT
	///              [11:02] = Ignored, 0
	///         else if([01:00] == D3D10_SB_OPERAND_4_COMPONENT
	///         {
	///              [03:02] = D3D10_SB_OPERAND_4_COMPONENT_SELECTION_MODE
	///              if([03:02] == D3D10_SB_OPERAND_4_COMPONENT_MASK_MODE)
	///              {
	///                  [07:04] = D3D10_SB_OPERAND_4_COMPONENT_MASK
	///                  [11:08] = Ignored, 0
	///              }
	///              else if([03:02] == D3D10_SB_OPERAND_4_COMPONENT_SWIZZLE_MODE)
	///              {
	///                  [11:04] = D3D10_SB_4_COMPONENT_SWIZZLE
	///              }
	///              else if([03:02] == D3D10_SB_OPERAND_4_COMPONENT_SELECT_1_MODE)
	///              {
	///                  [05:04] = D3D10_SB_4_COMPONENT_NAME
	///                  [11:06] = Ignored, 0
	///              }
	///         }
	///         else if([01:00] == D3D10_SB_OPERAND_N_COMPONENT)
	///         {
	///              Currently not defined.
	///         }
	/// [19:12] D3D10_SB_OPERAND_TYPE
	/// [21:20] D3D10_SB_OPERAND_INDEX_DIMENSION:
	///            Number of dimensions in the register
	///            file (NOT the # of dimensions in the
	///            individual register or memory
	///            resource being referenced).
	/// [24:22] if( [21:20] >= D3D10_SB_OPERAND_INDEX_1D )
	///             D3D10_SB_OPERAND_INDEX_REPRESENTATION for first operand index
	///         else
	///             Ignored, 0
	/// [27:25] if( [21:20] >= D3D10_SB_OPERAND_INDEX_2D )
	///             D3D10_SB_OPERAND_INDEX_REPRESENTATION for second operand index
	///         else
	///             Ignored, 0
	/// [30:28] if( [21:20] == D3D10_SB_OPERAND_INDEX_3D )
	///             D3D10_SB_OPERAND_INDEX_REPRESENTATION for third operand index
	///         else
	///             Ignored, 0
	/// [31]    0 normally. 1 if extended operand definition, meaning next DWORD
	///         contains extended operand description.
	/// </summary>
	public class Operand
	{
		private readonly OpcodeType _parentType;

		public byte NumComponents { get; internal set; }
		public Operand4ComponentSelectionMode SelectionMode { get; internal set; }
		public ComponentMask ComponentMask { get; internal set; }
		public Operand4ComponentName[] Swizzles { get; private set; }
		public OperandType OperandType { get; internal set; }
		public OperandIndexDimension IndexDimension { get; internal set; }
		// TODO: Can merge this with Indices?
		public OperandIndexRepresentation[] IndexRepresentations { get; private set; }
		public bool IsExtended { get; internal set; }
		public OperandModifier Modifier { get; internal set; }
		public OperandIndex[] Indices { get; private set; }
		public Number[] ImmediateValues { get; private set; }

		public Operand(OpcodeType parentType)
		{
			_parentType = parentType;
			Swizzles = new[]
			{
				Operand4ComponentName.X,
				Operand4ComponentName.Y,
				Operand4ComponentName.Z,
				Operand4ComponentName.W
			};
			IndexRepresentations = new OperandIndexRepresentation[3];
			Indices = new OperandIndex[3];
			ImmediateValues = new Number[4];
		}

		public static Operand Parse(BytecodeReader reader, OpcodeType parentType)
		{
			uint token0 = reader.ReadUInt32();

			var operand = new Operand(parentType);

			var numComponents = token0.DecodeValue<OperandNumComponents>(0, 1);
			switch (numComponents)
			{
				case OperandNumComponents.Zero:
					{
						operand.NumComponents = 0;
						break;
					}
				case OperandNumComponents.One:
					{
						operand.NumComponents = 1;
						break;
					}
				case OperandNumComponents.Four:
					{
						operand.NumComponents = 4;
						operand.SelectionMode = token0.DecodeValue<Operand4ComponentSelectionMode>(2, 3);
						switch (operand.SelectionMode)
						{
							case Operand4ComponentSelectionMode.Mask:
								{
									operand.ComponentMask = token0.DecodeValue<ComponentMask>(4, 7);
									break;
								}
							case Operand4ComponentSelectionMode.Swizzle:
								{
									var swizzle = token0.DecodeValue(4, 11);
									Func<uint, byte, Operand4ComponentName> swizzleDecoder = (s, i) =>
										(Operand4ComponentName) ((s >> (i * 2)) & 3);
									operand.Swizzles[0] = swizzleDecoder(swizzle, 0);
									operand.Swizzles[1] = swizzleDecoder(swizzle, 1);
									operand.Swizzles[2] = swizzleDecoder(swizzle, 2);
									operand.Swizzles[3] = swizzleDecoder(swizzle, 3);
									break;
								}
							case Operand4ComponentSelectionMode.Select1:
								{
									var swizzle = token0.DecodeValue<Operand4ComponentName>(4, 5);
									operand.Swizzles[0] = operand.Swizzles[1] = operand.Swizzles[2] = operand.Swizzles[3] = swizzle;
									break;
								}
							default:
								{
									throw new ArgumentException("Unrecognized selection method: " + operand.SelectionMode);
								}
						}
						break;
					}
				case OperandNumComponents.N:
					{
						throw new ArgumentException("OperandNumComponents.N is not currently supported.");
					}
			}

			operand.OperandType = token0.DecodeValue<OperandType>(12, 19);
			operand.IndexDimension = token0.DecodeValue<OperandIndexDimension>(20, 21);

			Func<uint, byte, OperandIndexRepresentation> indexRepresentationDecoder = (t, i) =>
				(OperandIndexRepresentation) t.DecodeValue((byte) (22 + (i * 3)), (byte) (22 + (i * 3) + 2));
			for (byte i = 0; i < (byte) operand.IndexDimension; i++)
				operand.IndexRepresentations[i] = indexRepresentationDecoder(token0, i);

			operand.IsExtended = token0.DecodeValue(31, 31) == 1;
			if (operand.IsExtended)
				ReadExtendedOperand(operand, reader);

			for (byte i = 0; i < (byte) operand.IndexDimension; i++)
			{
				operand.Indices[i] = new OperandIndex();
				switch (operand.IndexRepresentations[i])
				{
					case OperandIndexRepresentation.Immediate32:
						operand.Indices[i].Value = reader.ReadUInt32();
						break;
					case OperandIndexRepresentation.Immediate64:
						operand.Indices[i].Value = reader.ReadUInt64();
						goto default;
					case OperandIndexRepresentation.Relative:
						operand.Indices[i].Register = Parse(reader, parentType);
						break;
					case OperandIndexRepresentation.Immediate32PlusRelative:
						operand.Indices[i].Value = reader.ReadUInt32();
						goto case OperandIndexRepresentation.Relative;
					case OperandIndexRepresentation.Immediate64PlusRelative:
						operand.Indices[i].Value = reader.ReadUInt64();
						goto case OperandIndexRepresentation.Relative;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			var numberType = parentType.GetNumberType();
			switch (operand.OperandType)
			{
				case OperandType.Immediate32:
					for (var i = 0; i < operand.NumComponents; i++)
						operand.ImmediateValues[i] = Number.Parse32(reader, numberType);
					break;
				case OperandType.Immediate64:
					for (var i = 0; i < operand.NumComponents; i++)
						operand.ImmediateValues[i] = Number.Parse64(reader, numberType);
					break;
			}

			return operand;
		}

		/// <summary>
		/// Extended Instruction Operand Format (OperandToken1)
		///
		/// If bit31 of an operand token is set, the
		/// operand has additional data in a second DWORD
		/// directly following OperandToken0.  Other tokens
		/// expected for the operand, such as immmediate
		/// values or relative address operands (full
		/// operands in themselves) always follow
		/// OperandToken0 AND OperandToken1..n (extended
		/// operand tokens, if present).
		///
		/// [05:00] D3D10_SB_EXTENDED_OPERAND_TYPE
		/// [30:06] if([05:00] == D3D10_SB_EXTENDED_OPERAND_MODIFIER)
		///         {
		///              [13:06] D3D10_SB_OPERAND_MODIFIER
		///              [30:14] Ignored, 0.
		///         }
		///         else
		///         {
		///              [30:06] Ignored, 0.
		///         }
		/// [31]    0 normally. 1 if second order extended operand definition,
		///         meaning next DWORD contains yet ANOTHER extended operand
		///         description. Currently no second order extensions defined.
		///         This would be useful if a particular extended operand does
		///         not have enough space to store the required information in
		///         a single token and so is extended further.
		/// </summary>
		private static void ReadExtendedOperand(Operand operand, BytecodeReader reader)
		{
			uint token1 = reader.ReadUInt32();

			switch (token1.DecodeValue<ExtendedOperandType>(0, 5))
			{
				case ExtendedOperandType.Modifier:
					operand.Modifier = token1.DecodeValue<OperandModifier>(6, 13);
					break;
			}
		}

		public override string ToString()
		{
			var numberType = _parentType.GetNumberType();
			switch (OperandType)
			{
				case OperandType.Immediate32:
				{
					string result = (numberType == NumberType.Double) ? "d(" : "l(";
					bool addSpaces = _parentType != OpcodeType.Mov && _parentType != OpcodeType.MovC;
					for (int i = 0; i < NumComponents; i++)
					{
						result += ImmediateValues[i].ToString();

						if (i < NumComponents - 1)
						{
							result += ",";
							if (addSpaces)
								result += " ";
						}
					}
					result += ")";
					return result;
				}
				case OperandType.Null:
				{
					return OperandType.GetDescription();
				}
				default:
				{
					string index = string.Empty;
					switch (IndexDimension)
					{
						case OperandIndexDimension._0D:
							break;
						case OperandIndexDimension._1D:
							index = (IndexRepresentations[0] == OperandIndexRepresentation.Relative
								|| !OperandType.RequiresRegisterNumberFor1DIndex())
								? string.Format("[{0}]", Indices[0])
								: Indices[0].ToString();
							break;
						case OperandIndexDimension._2D :
							index = (IndexRepresentations[0] == OperandIndexRepresentation.Relative
								|| !OperandType.RequiresRegisterNumberFor2DIndex())
								? string.Format("[{0}][{1}]", Indices[0], Indices[1])
								: string.Format("{0}[{1}]", Indices[0], Indices[1]);
							break;
						case OperandIndexDimension._3D:
							break;
					}

					string components;
					switch (SelectionMode)
					{
						case Operand4ComponentSelectionMode.Mask:
							components = ComponentMask.GetDescription();
							break;
						case Operand4ComponentSelectionMode.Swizzle:
							components = Swizzles[0].GetDescription()
								+ Swizzles[1].GetDescription()
								+ Swizzles[2].GetDescription()
								+ Swizzles[3].GetDescription();
							break;
						case Operand4ComponentSelectionMode.Select1:
							components = Swizzles[0].GetDescription();
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					if (!string.IsNullOrEmpty(components))
						components = "." + components;

					return Modifier.Wrap(string.Format("{0}{1}{2}", OperandType.GetDescription(), index, components));
				}
			}
		}
	}
}