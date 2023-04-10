using EasyMapper.ExpressionParser;
using EasyMapper.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyMapper
{
    public static class EasyDealer
    {
        private static Assembly assem = Assembly.GetCallingAssembly();
        public static void Map<T>(T Target, object Source, Smilarities SimilarityType = Smilarities.MATCH, CasePreferences CasePreferencesType = CasePreferences.CASE_SENSITIVE, TypePreferences TypeSensitivity = TypePreferences.SENSITIVE) where T : class
        {
            Type[] tlist = assem.GetTypes();
            string classname = SimilarityType.ToString() + "_" + CasePreferencesType.ToString() + "_" + TypeSensitivity.ToString();
            Type t = assem.GetType("EasyMapper." + classname);
            MethodInfo mi = t.GetMethod("Map");
            mi = mi.MakeGenericMethod(new Type[] { typeof(T) });
            mi.Invoke(null, new object[] { Target, Source });
            
        }
        public static void MapBut<T>(T Target, object Source, Expression<Func<T, object>> predicate, Smilarities SimilarityType = Smilarities.MATCH, CasePreferences CasePreferencesType = CasePreferences.CASE_SENSITIVE, TypePreferences TypeSensitivity = TypePreferences.SENSITIVE) where T : class
        {
           string[] PropsNotToMap= PredicateResolver<T>(predicate);
            Type[] tlist = assem.GetTypes();
            string classname = SimilarityType.ToString() + "_" + CasePreferencesType.ToString() + "_" + TypeSensitivity.ToString();
            Type t = assem.GetType("EasyMapper." + classname);
            MethodInfo mi = t.GetMethod("MapBut");
            mi = mi.MakeGenericMethod(new Type[] { typeof(T) });
            mi.Invoke(null, new object[] { Target, Source , PropsNotToMap });
        }
        public static void MapOnly<T>(T Target, object Source, Expression<Func<T, object>> predicate, Smilarities SimilarityType = Smilarities.MATCH, CasePreferences CasePreferencesType = CasePreferences.CASE_SENSITIVE, TypePreferences TypeSensitivity = TypePreferences.SENSITIVE) where T : class
        {
            string[] ProppToMapOnly = PredicateResolver<T>(predicate);
            Type[] tlist = assem.GetTypes();
            string classname = SimilarityType.ToString() + "_" + CasePreferencesType.ToString() + "_" + TypeSensitivity.ToString();
            Type t = assem.GetType("EasyMapper." + classname);
            MethodInfo mi = t.GetMethod("MapOnly");
            mi = mi.MakeGenericMethod(new Type[] { typeof(T) });
            mi.Invoke(null, new object[] { Target, Source, ProppToMapOnly });
        }

        public static T Clone<T>(object Source) where T : class
        {
            T _obje = Activator.CreateInstance<T>();
            Map<T>(_obje, Source);
            return _obje;
        }
        public static T CloneBut<T>(object Source, Expression<Func<T, object>> predicate) where T : class
        {
            T _obje = Activator.CreateInstance<T>();
            MapBut<T>(_obje, Source, predicate);
            return _obje;
        }
        public static T CloneOnly<T>(object Source, Expression<Func<T, object>> predicate) where T : class
        {
            T _obje = Activator.CreateInstance<T>();
            MapOnly<T>(_obje, Source, predicate);
            return _obje;
        }
        private static string[] PredicateResolver<T>(Expression<Func<T, object>> predicate) where T : class
        {

            //bx.NodeType  bize recursive operatör metodu verecek
            if (predicate.Body.NodeType != ExpressionType.New) return null;

            NewExpression NewEx = (NewExpression)predicate.Body;
            string[] Properties = new string[NewEx.Arguments.Count];
            for (int i = 0; i < NewEx.Arguments.Count; i++)
            {
                MemberExpression m = (MemberExpression)NewEx.Arguments[i];
                Properties[i] = m.Member.Name;
            }
            return Properties;
        }

    }
    internal static class MATCH_CASE_SENSITIVE_SENSITIVE  
    {
        public static void MapOnly<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;
        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name == spip[sindex].Name && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    if (!MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }

                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
        public static void MapBut<T>(T Target, object Source, string[] MapButList)
        {
            try
            {
                List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
                List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
            start_base:
                int index = 0;
            start_first:
                int sindex = 0;

            start_second:
                if (pip.Count > index && spip.Count > sindex)
                {

                    if (pip[index].Name == spip[sindex].Name && pip[index].PropertyType == spip[sindex].PropertyType)
                    {
                        if (MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                        spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                    bypass:
                        spip.Remove(spip[sindex]);
                        pip.Remove(pip[index]);
                        goto start_base;
                    }

                    sindex++;
                    goto start_second;
                }
                else if (pip.Count > index)
                {
                    index++;
                    goto start_first;
                }
                else return;

            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public static void Map<T>(T Target, object Source)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name == spip[sindex].Name && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }
                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
    }
    internal static class MATCH_CASE_SENSITIVE_NON_SENSITIVE {

        public static void Map<T>(T Target, object Source)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;
        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name == spip[sindex].Name)
                {
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }
                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
        public static void MapOnly<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name == spip[sindex].Name)
                {
                    if (!MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }

                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
        public static void MapBut<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name == spip[sindex].Name )
                {
                    if (MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }
                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
    }
    internal static class MATCH_CASE_NON_SENSITIVE_SENSITIVE {

        public static void Map<T>(T Target, object Source)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name.ToLower() == spip[sindex].Name.ToLower() && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }
                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }

        public static void MapOnly<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {

                if (pip[index].Name.ToLower() == spip[sindex].Name.ToLower() && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    if (!MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }

                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
        public static void MapBut<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {

                if (pip[index].Name.ToLower() == spip[sindex].Name.ToLower() && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    if (MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }

                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
    }
    internal static class MATCH_CASE_NON_SENSITIVE_NON_SENSITIVE {

        public static void Map<T>(T Target, object Source)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {
                if (pip[index].Name.ToLower() == spip[sindex].Name.ToLower())
                {
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }
                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
        public static void MapOnly<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {

                if (pip[index].Name == spip[sindex].Name && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    if (MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }

                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
        public static void MapBut<T>(T Target, object Source, string[] MapButList)
        {
            List<PropertyInfo> pip = Source.GetType().GetProperties().ToList();
            List<PropertyInfo> spip = typeof(T).GetProperties().ToList();
        start_base:
            int index = 0;
        start_first:
            int sindex = 0;

        start_second:
            if (pip.Count > index && spip.Count > sindex)
            {

                if (pip[index].Name == spip[sindex].Name && pip[index].PropertyType == spip[sindex].PropertyType)
                {
                    if (MapButList.Any(m => m == spip[sindex].Name)) goto bypass;
                    //pip index bir type ise bir takim islemler yapilacak

                    spip[sindex].SetValue(Target, pip[index].GetValue(Source));
                bypass:
                    spip.Remove(spip[sindex]);
                    pip.Remove(pip[index]);
                    goto start_base;
                }

                sindex++;
                goto start_second;
            }
            else if (pip.Count > index)
            {
                index++;
                goto start_first;
            }
            else return;



        }
    }
   
}
