﻿using System.Collections.Generic;
using IcwUnits;
using IcwField;
using IcwBattle;
using IcwUI;

namespace IcwAI
{
    public interface ITeamAI
    {
        List<IUnit> Enemies { get; set; } // список противников
        List<IUnit> TeamMates { get; set; } // список союзников
        IField Field { get; set; } // поле 
        IBattle Battle { get; set; }    // бой в котором дерется команда
        IPresenter Presenter { get; set; } // презентер для вывода текста и визуала
        void DoOneTurn(); // Выполнить один ход
        void DoNewRound(); // Следующий раунд

        // Экипировать команду, здесь нужна ссылка список объектов IItem откуда выбирается экипировка
        // интерфейс IItem еще не сделан
        void DoEquip<T>(List<T> Items); 
    }
}
