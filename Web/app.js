async function loadGames() {
    const gamesContainer = document.getElementById("games");
    const gameCount = document.getElementById("game-count");

    try {
        const response = await fetch("/api/games");

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const games = await response.json();

        gameCount.textContent = `${games.length} jeux`;

        gamesContainer.innerHTML = "";

        for (const game of games) {
            const card = createGameCard(game);
            gamesContainer.appendChild(card);
        }
    }
    catch (error) {
        console.error("Impossible de charger les jeux :", error);

        gamesContainer.innerHTML = `
            <div class="error">
                Impossible de charger la bibliothèque.
            </div>
        `;

        gameCount.textContent = "Erreur";
    }
}


function createGameCard(game) {
    const card = document.createElement("article");
    card.className = "game-card";

    card.addEventListener("click", () => {
        console.log("Jeu sélectionné :", game);
    });

    if (game.cover) {
        const image = document.createElement("img");

        image.className = "game-cover";
        image.src = game.cover;
        image.alt = game.name;

        card.appendChild(image);
    }
    else {
        const noCover = document.createElement("div");

        noCover.className = "game-cover no-cover";
        noCover.textContent = "Pas de cover";

        card.appendChild(noCover);
    }

    const info = document.createElement("div");
    info.className = "game-info";

    const name = document.createElement("div");
    name.className = "game-name";
    name.textContent = game.name;

    info.appendChild(name);

    if (game.favorite) {
        const favorite = document.createElement("span");
        favorite.className = "favorite";
        favorite.textContent = "★";

        info.appendChild(favorite);
    }

    card.appendChild(info);

    return card;
}


loadGames();