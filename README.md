# ConnectDotsGame

Игра "Трубопровод" =)

## Структура проекта

- `Models/` — модели данных и игровая логика
  - `Level.cs` — модель уровня и механика проверки завершения
  - `Point.cs` — точки на игровом поле
  - `Line.cs` — линии, соединяющие точки
  - `GameState.cs` — состояние игры и прогресс

- `ViewModels/` — логика представления (MVVM)
  - `GameViewModel.cs` — управление игровым процессом
  - `LevelSelectViewModel.cs` — выбор уровней
  - `MainPageViewModel.cs` — главное меню
  - `AboutPageViewModel.cs` — страница "О игре"
  - `ModalWindowViewModel.cs` — модальные окна

- `Views/` — пользовательский интерфейс
  - Современный UI с градиентами и тенями
  - Адаптивный дизайн
  - Модальные окна для важных событий

- `Services/` — игровые сервисы
  - `GameService.cs` (IGameService) — игровая механика
  - `GameStorageService.cs` (IGameStorageService) — сохранение прогресса
  - `PathManager.cs` (IPathManager) — управление путями и линиями
  - `NavigationService.cs` (INavigation) — навигация между экранами
  - `ModalService.cs` (IModalService) — управление модальными окнами

#elki-palki
