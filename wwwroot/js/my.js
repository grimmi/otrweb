var loadedepisodes = [];
var currentepisode = {};
var loadedmovies = [];
var currentmovie = {};

function pad(number){
    var padded = number + "";
    if(padded.length === 1){
        padded = "0" + padded;
    }
    return padded;
}

function filledit(index){
    var editepisode = loadedepisodes[index];
    document.getElementById("editshow").value = editepisode["show"];
    document.getElementById("editseason").value = editepisode["season"];
    document.getElementById("editname").value = editepisode["name"];
    document.getElementById("editnumber").value = editepisode["number"];
}