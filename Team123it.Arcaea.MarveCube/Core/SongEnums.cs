namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示曲目的难度。
	/// </summary>
	public enum SongDifficulty
	{
		/// <summary>
		/// Past难度。
		/// </summary>
		Past = 0,
		/// <summary>
		/// Present难度。
		/// </summary>
		Present = 1,
		/// <summary>
		/// Future难度。
		/// </summary>
		Future = 2,
		/// <summary>
		/// Beyond难度。
		/// </summary>
		Beyond = 3
	}

	/// <summary>
	/// 表示曲目完成的类型。
	/// </summary>
	public enum ClearType
	{
		/// <summary>
		/// [TL]曲目失败(Track Lost)。
		/// </summary>
		TrackLost = 0,
		/// <summary>
		/// [EC]简单回忆条通关(Track Complete)。
		/// </summary>
		EasyClear = 4,
		/// <summary>
		/// [NC]普通回忆条通关(Track Complete)。
		/// </summary>
		NormalClear = 1,
		/// <summary>
		/// [HC]困难回忆条通关(Track Complete)。
		/// </summary>
		HardClear = 5,
		/// <summary>
		/// [FR]全部连击(Full Recall)。
		/// </summary>
		FullRecall = 2,
		/// <summary>
		/// [PM]全部完美(Pure Memory)。
		/// </summary>
		PureMemory = 3
	}

	/// <summary>
	/// 表示曲目完成的评级。
	/// </summary>
	public enum GradeType
	{
		/// <summary>
		/// EX+评级(分数在990w及以上)。
		/// </summary>
		EX_Plus = 6,
		/// <summary>
		/// EX评级(分数在980w-9899999区间)。
		/// </summary>
		EX = 5,
		/// <summary>
		/// AA评级(分数950w-9799999区间)。
		/// </summary>
		AA = 4,
		/// <summary>
		/// A评级(分数920w-9499999区间)。
		/// </summary>
		A = 3,
		/// <summary>
		/// B评级(分数890w-9199999区间)。
		/// </summary>
		B = 2,
		/// <summary>
		/// C评级(分数860w-8899999区间)。
		/// </summary>
		C = 1,
		/// <summary>
		/// D评级(分数在8599999及以下)。
		/// </summary>
		D = 0
	}

	/// <summary>
	/// 提供适用于 <see cref="SongEnums"/> 系列枚举的静态方法的类。无法继承此类。
	/// </summary>
	public static class SongEnumsStatics
	{
		/// <summary>
		/// 将指定的分数转换为对应的 <see cref="GradeType"/> 评级。
		/// </summary>
		/// <param name="score">要转换的分数。</param>
		/// <returns>转换结果。</returns>
		public static GradeType ConvertScoreToGrade(uint score)
		{
			if (score >= 9900000) return GradeType.EX_Plus;
			else if (score >= 9800000 && score < 9900000) return GradeType.EX;
			else if (score >= 9500000 && score < 9800000) return GradeType.AA;
			else if (score >= 9200000 && score < 9500000) return GradeType.A;
			else if (score >= 8900000 && score < 9200000) return GradeType.B;
			else if (score >= 8600000 && score < 8900000) return GradeType.C;
			else return GradeType.D;
		}

		/// <summary>
		/// 检查当前 <see cref="ClearType"/> 对应的曲目完成类型是否高于另一个 <see cref="ClearType"/> 对应的曲目类型。
		/// </summary>
		/// <param name="clearType1">当前 <see cref="ClearType"/>。</param>
		/// <param name="clearType2">要判断的另一个 <see cref="ClearType"/>。</param>
		/// <returns>当前 <see cref="ClearType" /> 高于另一个 <see cref="ClearType"/> 则为 <see langword="true"/> ; 否则为 <see langword="false"/> 。</returns>
		public static bool CheckIsHigher(this ClearType clearType1,ClearType clearType2)
		{
			switch (clearType1)
			{
				case ClearType.PureMemory:
					if (clearType2 == ClearType.PureMemory) return false;
					else return true;
				case ClearType.FullRecall:
					if (clearType2 == ClearType.PureMemory || clearType2 == ClearType.FullRecall) return false;
					else return true;
				case ClearType.HardClear:
					if (clearType2 == ClearType.PureMemory || clearType2 == ClearType.FullRecall || clearType2 == ClearType.HardClear) return false;
					else return true;
				case ClearType.NormalClear:
					if (clearType2 == ClearType.TrackLost || clearType2 == ClearType.EasyClear) return true;
					else return false;
				case ClearType.EasyClear:
					if (clearType2 == ClearType.TrackLost) return true;
					else return false;
				case ClearType.TrackLost:
					return false;
				default:
					return false;
			}
		}
	}
}
