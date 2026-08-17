using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace OrangePeelGGC2Mods.CharacterTraits.Patches
{
    /// <summary>
    /// 蒸汽朋克文尼三段跳补丁
    /// 实现原理：在二段跳或空中翻滚后，重置 hasDoubleJumped 标志，允许再跳一次
    /// 使用外部字典跟踪额外跳跃状态，避免修改原始字段
    /// </summary>
    [HarmonyPatch]
    public static class SteampunkVinnieTripleJumpPatch
    {
        // 缓存反射字段信息，避免重复查找
        private static FieldInfo unitField = AccessTools.Field(typeof(UnitMover), "unit");
        private static FieldInfo hasDoubleJumpedField = AccessTools.Field(typeof(UnitMover), "hasDoubleJumped");
        
        // 使用字典跟踪每个 UnitMover 实例的额外跳跃状态
        // 键：UnitMover 实例；值：是否已使用额外跳跃
        private static Dictionary<UnitMover, bool> hasModExtraJumped = new Dictionary<UnitMover, bool>();

        /// <summary>
        /// 判断是否应该重置二段跳标志
        /// 仅当满足以下所有条件时返回 true：
        /// 1. 当前单位是蒸汽朋克文尼
        /// 2. 尚未使用额外跳跃
        /// </summary>
        private static bool ShouldResetDoubleJump(UnitMover mover)
        {
            try
            {
                // 空值检查
                if (ReferenceEquals(mover, null) || ReferenceEquals(unitField, null))
                    return false;
                
                // 获取 Unit 实例
                Unit unit = unitField.GetValue(mover) as Unit;
                if (ReferenceEquals(unit, null))
                    return false;
                
                // 检查是否为蒸汽朋克文尼
                bool isSteampunkVinnie = unit.UnitType == UnitTypeData.steampunkVinnie;
                if (!isSteampunkVinnie)
                    return false;
                
                // 检查是否已使用额外跳跃
                bool extraJumped = false;
                if (hasModExtraJumped.ContainsKey(mover))
                    extraJumped = hasModExtraJumped[mover];
                
                // 只有未使用额外跳跃时才重置
                return !extraJumped;
            }
            catch
            {
                // 任何异常都返回 false，确保不影响其他单位
                return false;
            }
        }

        /// <summary>
        /// 标记已使用额外跳跃
        /// </summary>
        private static void SetModExtraJumped(UnitMover mover)
        {
            try
            {
                hasModExtraJumped[mover] = true;
            }
            catch
            {
                // 忽略异常
            }
        }

        /// <summary>
        /// 重置额外跳跃标志（落地时调用）
        /// </summary>
        private static void ResetModExtraJumped(UnitMover mover)
        {
            try
            {
                if (hasModExtraJumped.ContainsKey(mover))
                {
                    hasModExtraJumped.Remove(mover);
                }
            }
            catch
            {
                // 忽略异常
            }
        }

        /// <summary>
        /// JumpUpdate 的 Transpiler
        /// 在 hasDoubleJumped = true 赋值后插入逻辑：
        /// 如果是蒸汽朋克文尼且未使用额外跳跃，则重置 hasDoubleJumped = false
        /// </summary>
        [HarmonyPatch(typeof(UnitMover), "JumpUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> JumpUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            
            // 遍历指令，查找 hasDoubleJumped 字段的赋值
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && 
                    ReferenceEquals(codes[i].operand, hasDoubleJumpedField))
                {
                    // 创建新的指令列表
                    var newInstructions = new List<CodeInstruction>();
                    
                    // 保留原指令：hasDoubleJumped = true
                    newInstructions.Add(codes[i]);
                    
                    // 调用 ShouldResetDoubleJump(this)
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(CodeInstruction.Call(typeof(SteampunkVinnieTripleJumpPatch), nameof(ShouldResetDoubleJump)));
                    
                    // 如果返回 false，跳过重置逻辑
                    var skipLabel = new Label();
                    newInstructions.Add(new CodeInstruction(OpCodes.Brfalse, skipLabel));
                    
                    // this.hasDoubleJumped = false（重置二段跳标志）
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                    newInstructions.Add(new CodeInstruction(OpCodes.Stfld, hasDoubleJumpedField));
                    
                    // SetModExtraJumped(this)（标记已使用额外跳跃）
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(CodeInstruction.Call(typeof(SteampunkVinnieTripleJumpPatch), nameof(SetModExtraJumped)));
                    
                    // 添加跳转标签到最后一条指令
                    newInstructions[newInstructions.Count - 1].labels.Add(skipLabel);
                    
                    // 替换原指令
                    codes.RemoveAt(i);
                    codes.InsertRange(i, newInstructions);
                    
                    break;
                }
            }
            
            return codes;
        }

        /// <summary>
        /// StartRolling 的 Transpiler
        /// 在 hasDoubleJumped = true 赋值后插入逻辑：
        /// 如果是蒸汽朋克文尼且未使用额外跳跃，则重置 hasDoubleJumped = false
        /// </summary>
        [HarmonyPatch(typeof(UnitMover), "StartRolling")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> StartRollingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            
            // 遍历指令，查找 hasDoubleJumped 字段的赋值
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && 
                    ReferenceEquals(codes[i].operand, hasDoubleJumpedField))
                {
                    // 创建新的指令列表
                    var newInstructions = new List<CodeInstruction>();
                    
                    // 保留原指令：hasDoubleJumped = true
                    newInstructions.Add(codes[i]);
                    
                    // 调用 ShouldResetDoubleJump(this)
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(CodeInstruction.Call(typeof(SteampunkVinnieTripleJumpPatch), nameof(ShouldResetDoubleJump)));
                    
                    // 如果返回 false，跳过重置逻辑
                    var skipLabel = new Label();
                    newInstructions.Add(new CodeInstruction(OpCodes.Brfalse, skipLabel));
                    
                    // this.hasDoubleJumped = false（重置二段跳标志）
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                    newInstructions.Add(new CodeInstruction(OpCodes.Stfld, hasDoubleJumpedField));
                    
                    // SetModExtraJumped(this)（标记已使用额外跳跃）
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(CodeInstruction.Call(typeof(SteampunkVinnieTripleJumpPatch), nameof(SetModExtraJumped)));
                    
                    // 添加跳转标签到最后一条指令
                    newInstructions[newInstructions.Count - 1].labels.Add(skipLabel);
                    
                    // 替换原指令
                    codes.RemoveAt(i);
                    codes.InsertRange(i, newInstructions);
                    
                    break;
                }
            }
            
            return codes;
        }

        /// <summary>
        /// ChangeGroundedState 的 Transpiler
        /// 在 hasDoubleJumped = false 重置后插入逻辑：
        /// 重置额外跳跃标志，使蒸汽朋克文尼在下次空中时能再次使用三段跳
        /// </summary>
        [HarmonyPatch(typeof(UnitMover), "ChangeGroundedState")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeGroundedStateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            
            // 遍历指令，查找 hasDoubleJumped 字段的赋值
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && 
                    ReferenceEquals(codes[i].operand, hasDoubleJumpedField))
                {
                    // 在原指令后插入重置调用
                    var insertCode = new List<CodeInstruction>
                    {
                        // this
                        new CodeInstruction(OpCodes.Ldarg_0),
                        // ResetModExtraJumped(this)
                        CodeInstruction.Call(typeof(SteampunkVinnieTripleJumpPatch), nameof(ResetModExtraJumped)),
                    };
                    
                    codes.InsertRange(i + 1, insertCode);
                    break;
                }
            }
            
            return codes;
        }
    }
}