function pad(number){
    var padded = number + "";
    if(padded.length === 1){
        padded = "0" + padded;
    }
    return padded;
}