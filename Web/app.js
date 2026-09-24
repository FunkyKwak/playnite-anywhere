console.log("=== APP.JS CHARGÉ ===", Date.now());

let games = [];
window.preferences = {
    GroupBy: "None",
    CollapsedGroups: {}
};
let currentGroupBy = "None";

async function loadPreferences() {
    const response = await fetch("/api/preferences");

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
    }

    window.preferences = await response.json();

    console.log("1 - Préférences récupérées :", window.preferences);
    console.log("preferences.GroupBy :", window.preferences.GroupBy);
    console.log("preferences.CollapsedGroups :", window.preferences.CollapsedGroups);

    currentGroupBy = window.preferences.GroupBy || "None";
    document.getElementById("group-by").value = currentGroupBy;

    //if (!window.preferences.CollapsedGroups) {
    //    window.preferences.CollapsedGroups = {};
    //}

    console.log("2 - Fin loadPreferences :", preferences, "window.preferences :", window.preferences);
}

async function saveGroupState(groupName, collapsed) {
    const response = await fetch("/api/preferences/groups", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Name: groupName,
            Collapsed: collapsed
        })
    });

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
    }
}
async function savePreferences(groupBy) {
    const response = await fetch("/api/preferences", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            GroupBy: groupBy
        })
    });

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
    }
}

async function loadGames() {
    console.log("3 - Début loadGames :", preferences, "window.preferences :", window.preferences);

    const gamesContainer = document.getElementById("games");
    const gameCount = document.getElementById("game-count");

    try {
        const response = await fetch("/api/games");

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        games = await response.json();

        gameCount.textContent = `${games.length} jeux`;

        console.log("4 - Avant renderGames :", window.preferences);
        renderGames();
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

function renderGames() {
    const gamesContainer = document.getElementById("games");

    gamesContainer.innerHTML = "";

    if (currentGroupBy === "None") {
        const grid = createGamesGrid(games);
        gamesContainer.appendChild(grid);
        return;
    }

    const groups = groupGames(games, currentGroupBy);

    console.log(window.preferences);
    console.log(window.preferences.CollapsedGroups);
    for (const [groupName, groupGamesList] of groups) {

        const section = document.createElement("section");
        section.className = "game-group";

        console.log(
            "Groupe:",
            groupName,
            "collapsed:",
            window.preferences.CollapsedGroups?.[groupName]
        );
        const isCollapsed = window.preferences.CollapsedGroups?.[groupName] === true;
        if (isCollapsed) {
            section.classList.add("collapsed");
        }

        const header = document.createElement("button");
        header.className = "game-group-header";

        const title = document.createElement("div");
        title.className = "game-group-title";

        const arrow = document.createElement("span");
        arrow.className = "game-group-arrow";
        arrow.textContent = "▼";

        const name = document.createElement("span");
        name.textContent = groupName;

        const count = document.createElement("span");
        count.className = "game-group-count";
        count.textContent = groupGamesList.length;

        title.appendChild(arrow);
        title.appendChild(name);
        title.appendChild(count);

        header.appendChild(title);

        const grid = createGamesGrid(groupGamesList);

        header.addEventListener("click", async () => {

            const collapsed =
                !section.classList.contains("collapsed");

            section.classList.toggle("collapsed");

            preferences.CollapsedGroups[groupName] = collapsed;

            try {
                await saveGroupState(groupName, collapsed);
            }
            catch (error) {
                console.error(
                    "Impossible d'enregistrer l'état du groupe :",
                    error
                );
            }
        });

        section.appendChild(header);
        section.appendChild(grid);

        gamesContainer.appendChild(section);
    }
}

function groupGames(games, groupBy) {
    const groups = new Map();

    for (const game of games) {

        let groupName;

        switch (groupBy) {

            case "Source":
                groupName = game.source || "Source inconnue";
                break;

            case "Progress":
                groupName = game.completionStatus || "Non défini";
                break;

            case "Platform":
                groupName = game.platforms?.join(", ") || "Plateforme inconnue";
                break;

            default:
                groupName = "Autres";
                break;
        }

        if (!groups.has(groupName)) {
            groups.set(groupName, []);
        }

        groups.get(groupName).push(game);
    }

    return [...groups.entries()].sort((a, b) =>
        a[0].localeCompare(b[0])
    );
}

function createGamesGrid(gamesList) {
    const grid = document.createElement("div");

    grid.className = "games-grid";

    for (const game of gamesList) {
        const card = createGameCard(game);

        grid.appendChild(card);
    }

    return grid;
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

async function changeGrouping(event) {
    const newGroupBy = event.target.value;

    currentGroupBy = newGroupBy;

    renderGames();

    try {
        await savePreferences(newGroupBy);
    }
    catch (error) {
        console.error(
            "Impossible d'enregistrer le regroupement :",
            error
        );
    }
}

document
    .getElementById("group-by")
    .addEventListener("change", changeGrouping);

async function initialize() {
    try {
        await loadPreferences();
        await loadGames();
    }
    catch (error) {
        console.error(
            "Impossible d'initialiser Playnite Anywhere :",
            error
        );
    }
}

initialize();