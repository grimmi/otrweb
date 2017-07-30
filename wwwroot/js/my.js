var loadedepisodes = [];
var currentepisode = {};
var loadedmovies = [];
var currentmovie = {};

function filterEpisodes(info) {
    return info["type"] === "episode";
}

function filterMovies(info) {
    return info["type"] === "movie";
}

function compareMovies(a, b) {
    return a["name"].toLowerCase() > b["name"].toLowerCase() ? 1 : a["name"] === b["name"] ? 0 : -1;
}

function compare(a, b) {
    if (a === b) { return 0; }
    else if (a > b) { return 1; }
    return -1;
}

function compareEpisodes(a, b) {
    var showCompare = compare(a["show"].toLowerCase(), b["show"].toLowerCase());
    if (showCompare != 0) { return showCompare; }
    var seasonCompare = compare(a["season"], b["season"]);
    if (seasonCompare != 0) { return seasonCompare; }
    var numberCompare = compare(a["number"], b["number"]);
    return numberCompare;
}

function printInfos(response) {
    var episodes = response["files"].filter(filterEpisodes).sort(compareEpisodes);
    loadedepisodes = episodes;
    printepisodes(episodes);
    var movies = response["files"].filter(filterMovies).sort();
    loadedmovies = movies;
    printmovies(movies);
}

function printepisodes(episodes) {
    document.getElementById("container").innerHTML = "";
    for (var i = 0; i < episodes.length; i++) {
        var episode = episodes[i];
        episode["season"] = episode["season"] === -1 ? "?" : pad(episode["season"]);
        episode["number"] = episode["number"] === -1 ? "?" : pad(episode["number"]);
        episode["class"] = i % 2 == 0 ? "evenrow" : "oddrow";
        episode["index"] = i;
        document.getElementById("container").innerHTML += tmpl("tmpl-episode", episode);
    }
}

function printmovies(movies){
    document.getElementById("moviecontainer").innerHTML = "";
    for (var i = 0; i < movies.length; i++) {
        var movie = movies[i];
        movie["class"] = i % 2 == 0 ? "evenrow" : "oddrow";
        movie["index"] = i;
        document.getElementById("moviecontainer").innerHTML += tmpl("tmpl-movie", movie);
    }
}

function pad(number) {
    var padded = number + "";
    if (padded.length === 1) {
        padded = "0" + padded;
    }
    return padded;
}

function filledit(index) {
    currentepisode = loadedepisodes[index];
    document.getElementById("editshow").value = currentepisode["show"];
    document.getElementById("editseason").value = currentepisode["season"];
    document.getElementById("editname").value = currentepisode["name"];
    document.getElementById("editnumber").value = currentepisode["number"];
    document.getElementById("closepopup").setAttribute("href", "#ep-" + currentepisode["index"]);    
    document.getElementById("cancelpopup").setAttribute("href", "#ep-" + currentepisode["index"]);
}

function applyepisodeedits(){
    currentepisode["show"] = document.getElementById("editshow").value;
    currentepisode["season"] = document.getElementById("editseason").value;
    currentepisode["name"] = document.getElementById("editname").value;
    currentepisode["number"] = document.getElementById("editnumber").value;
    loadedepisodes[currentepisode["index"]] = currentepisode;
    printepisodes(loadedepisodes);
    currentepisode = {};
}

function fillmovieedit(index) {
    currentmovie = loadedmovies[index];
    document.getElementById("editmoviename").value = currentmovie["name"];
    document.getElementById("closemoviepopup").setAttribute("href", "#mov-" + currentmovie["index"]);
    document.getElementById("cancelmoviepopup").setAttribute("href", "#mov-" + currentmovie["index"]);
}

function applymovieedits() {
    currentmovie["name"] = document.getElementById("editmoviename").value;
    loadedmovies[currentmovie["index"]] = currentmovie;
    printmovies(loadedmovies);
    currentmovie = {};
}