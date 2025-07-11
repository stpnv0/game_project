# Игра "Трубопровод" =)



## Функционал

### Игровая механика
- Соединение точек одинакового цвета линиями
- Предотвращение пересечения линий
- Система проверки завершения уровня
- Возможность отмены последнего действия
- Автоматическое сохранение прогресса

### Уровни
- 10 предустановленных уровней разной сложности
- Система прогрессии (новые уровни открываются после прохождения предыдущих)
- Загрузка уровней из JSON файла
- Сохранение прогресса прохождения

### Интерфейс
- Индикация заблокированных уровней
- Навигация между уровнями
- Визуальная обратная связь при взаимодействии

## Технологии
- C# (.NET 9)
- Avalonia UI для кроссплатформенного GUI
- JSON для хранения данных уровней
- MVVM архитектура

## Структура проекта
- `Models/` - игровые сущности и состояния
- `ViewModels/` - модели представления
- `Views/` - UI компоненты и разметка
- `Levels/` - система загрузки уровней
- `Services/` - игровые сервисы
- `Images/` - графические ресурсы


## Разработка
Для добавления новых уровней отредактируйте файл `Levels/levels.json`. Каждый уровень описывается следующими параметрами:
- `id` - уникальный идентификатор
- `name` - название уровня
- `rows`, `columns` - размеры сетки
- `colorPoints` - массив цветных точек с координатами и цветом

