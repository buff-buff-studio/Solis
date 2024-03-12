using MoonSharp.Interpreter;
using UnityEngine;

namespace SolarBuff
{
	public class ScriptingTest : MonoBehaviour
	{
		void OnEnable()
		{
			var script = @"    
		-- defines a factorial function
		function fact (n)
			if (n == 0) then
				return 1
			else
				return n*fact(n - 1)
			end
		end

		return fact(5)";

			var res = Script.RunString(script);
			Debug.Log(res.Number);
		}
	}
}