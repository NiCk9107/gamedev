# Игровые системы — Ыйман

Game.unity уже содержит полный Level_01, GameCanvas, один GameManager, CandleFlame на Player, свет и EventSystem нового Input System. Повторно добавлять эти компоненты не нужно.

| Интерфейс | Назначение |
| --- | --- |
| GameManager.Instance.State | Ready / Playing / Won / Lost |
| StateChanged | Action<GameState> для UI |
| StartGame() | Старт из Ready |
| Win() / Lose() | Завершение только из Playing |
| Restart() | Перезагрузка сцены, возврат в меню Ready |
| CandleFlame.Value / MaxFlame | Текущее / максимальное пламя |
| CandleFlame.Normalized | 0–1 для света и HUD |
| Restore(25f) | Воск |
| Hurt(20f) / Hurt(35f) | Ветер / тень |

В меню и после финала время остановлено. При нуле или падении ниже Y=−15 наступает проигрыш. Кнопки работают при остановленном игровом времени.

Обработчики воска, опасностей и алтаря находятся в исходных префабах. Не добавляйте дубли на экземпляры сцены. Параметры — в BALANCE.md, итоги и оставшиеся задачи команды — в YIMAN_ROLE_STATUS.md.

Проверки: Window → General → Test Runner → PlayMode → Run All. После изменений UI, уровня или P0 повторить нужные сценарии на итоговой сборке. В папке Tests также предусмотрено восстановление часов после тестов, чтобы отчёт Windows-плеера мог отправиться после паузы на финальном экране.
