(() => {
    let clres=document.getElementsByClassName("generator_color_hex");
    let clrnms=document.getElementsByClassName("generator_color_color-info");
    let cpnm = prompt("What should this palette be called?");
    let dtxt=cpnm
    +"\n"+clrnms[0].innerText+"\n"+clres[0].innerText
    +"\n"+clrnms[1].innerText+"\n"+clres[1].innerText
    +"\n"+clrnms[2].innerText+"\n"+clres[2].innerText
    +"\n"+clrnms[3].innerText+"\n"+clres[3].innerText
    +"\n"+clrnms[4].innerText+"\n"+clres[4].innerText;
    let celery=document.createElement("a");
    celery.setAttribute('href','data:text/plain;charset=utf-8,'+encodeURIComponent(dtxt));
    celery.setAttribute('download',cpnm+".sqwd");
    celery.style.display='none';
    document.body.appendChild(celery);
    celery.click();
    document.body.removeChild(celery);
})();
