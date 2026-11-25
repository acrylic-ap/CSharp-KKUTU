using System;
using System.Collections.Generic;
using System.Text;

namespace UI.Forms
{
    public interface IGame
    {
        void Initialize();

        void ShowWord(string word);

        Task<bool> IsGameOver();

        Task<bool> CompleteTurn(string word);

        void GameOver();
    }
}