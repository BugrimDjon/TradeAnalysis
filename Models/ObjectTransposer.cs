using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace bot_analysis.Models
{
    public class TransposedRow
    {
        public string Label { get; set; }
        public List<string> Values { get; set; } = new List<string>();
    }

   

    public class TransposedItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public static class ObjectTransposer
    {
        public static List<TransposedRow> TransposeObjects(IEnumerable<object> objects)
        {
            var listObjects = objects.ToList();
            if (!listObjects.Any()) return new List<TransposedRow>();

            // Получаем все уникальные имена свойств (Label) из первого объекта
            var labels = ObjectTransposer.TransposeObject(listObjects[0])
                                       .Select(t => t.Label)
                                       .ToList();

            // Для каждого объекта создаём словарь label->value
            var allValues = listObjects
                .Select(obj => ObjectTransposer.TransposeObject(obj)
                    .ToDictionary(t => t.Label, t => t.Value))
                .ToList();

            var result = new List<TransposedRow>();

            foreach (var label in labels)
            {
                var row = new TransposedRow { Label = label };
                foreach (var dict in allValues)
                {
                    dict.TryGetValue(label, out var value);
                    row.Values.Add(value ?? "");
                }
                result.Add(row);
            }

            return result;
        }


        public static List<TransposedItem> TransposeObject(object obj, string parentPrefix = "")
        {
            var result = new List<TransposedItem>();

            if (obj == null) return result;

            var props = obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)
                .Select(p => new
                {
                    Prop = p,
                    Display = p.GetCustomAttribute<DisplayAttribute>(),
                    Order = p.GetCustomAttribute<DisplayAttribute>()?.GetOrder() ?? int.MaxValue,
                    Name = p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name
                })
                .OrderBy(x => x.Order);

            foreach (var propInfo in props)
            {
                var value = propInfo.Prop.GetValue(obj);
                string labelPrefix = string.IsNullOrEmpty(parentPrefix) ? "" : parentPrefix + " > ";
                string fullLabel = labelPrefix + propInfo.Name;

                if (value != null && !IsSimpleType(propInfo.Prop.PropertyType))
                {
                    // Рекурсивно добавляем вложенные свойства с новым префиксом
                    var nestedLabel = labelPrefix + (propInfo.Display?.Name ?? propInfo.Name);
                    var nested = TransposeObject(value, nestedLabel);
                    result.AddRange(nested);
                }
                else
                {
                    result.Add(new TransposedItem
                    {
                        Label = labelPrefix + (propInfo.Display?.Name ?? propInfo.Name),
                        Value = value?.ToString() ?? ""
                    });
                }
            }

            return result;
        }


        private static bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                new Type[]
                {
                    typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset),
                    typeof(TimeSpan), typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}
