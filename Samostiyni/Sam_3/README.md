# Звіт з аналізу інкапсуляції в Open-Source проєкті

## 1. Обраний проєкт
- **Назва:** Newtonsoft.Json (Json.NET)
- **Посилання на GitHub:** https://github.com/JamesNK/Newtonsoft.Json

---

## 2. Аналіз інкапсуляції

### Клас: JsonSerializerSettings
- **Посилання на файл:** https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/JsonSerializerSettings.cs
- **Опис класу:** Використовується для налаштування параметрів серіалізації та десеріалізації об'єктів у формат JSON.
- **Поля:** Поля класу є приватними (наприклад, `_maxDepth`, `_culture`), що запобігає прямому модифікуванню внутрішнього стану об'єкта ззовні.
- **Властивості:** Клас використовує комбінацію автоматичних властивостей (`public Formatting Formatting { get; set; }`) та властивостей із явними backing-полями, де потрібна валідація значення при встановленні.
- **Індексатори/Оператори:** Відсутні, оскільки клас є конфігураційним контейнером даних.

### Клас: JObject
- **Посилання на файл:** https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Linq/JObject.cs
- **Опис класу:** Представляє JSON-об'єкт у вигляді колекції пар "ключ-значення".
- **Поля:** Внутрішні колекції для збереження властивостей оголошені як приватні або закриті всередині базового класу `JContainer`.
- **Властивості:** Властивість `Type` перевизначена як read-only (`public override JTokenType Type => JTokenType.Object;`).
- **Індексатори/Оператори:** Реалізовано індексатори для доступу до елементів об'єкта за ключем:
```csharp
public override JToken? this[object key]
{
    get => this[key as string];
    set
    {
        if (key is string propertyName)
        {
            this[propertyName] = value;
        }
    }
}

public JToken? this[string propertyName]
{
    get => Property(propertyName, StringComparison.Ordinal)?.Value;
    set
    {
        JProperty? property = Property(propertyName, StringComparison.Ordinal);
        if (property != null)
        {
            property.Value = value ?? JValue.CreateNull();
        }
        else
        {
            Add(new JProperty(propertyName, value));
        }
    }
}