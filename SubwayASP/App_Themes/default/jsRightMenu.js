function evtMenu1() {
    HideMenu();
    window.location.href = "GoogleMap.aspx?data=" + document.all("HdMapData").value;
}

function evtMenu3() {
    HideMenu();
    document.all("hdInvokeServer").click();
}


function evtMenuOnmouseMove() {
    this.style.backgroundColor = '#8AAD77';
    this.style.paddingLeft = '30px';
}

function evtOnMouseOut() {
    this.style.backgroundColor = '#FAFFF8';
}


function CreateMenu() {
    var div_Menu = document.createElement("Div");
    div_Menu.id = "div_RightMenu";
    div_Menu.className = "div_RightMenu";

    var div_Menu1 = document.createElement("Div");
    div_Menu1.className = "divMenuItem";
    div_Menu1.onclick = evtMenu1;
    div_Menu1.onmousemove = evtMenuOnmouseMove;
    div_Menu1.onmouseout = evtOnMouseOut;
    div_Menu1.innerHTML = "在Google地图定位";

    var div_Menu3 = document.createElement("Div");
    div_Menu3.className = "divMenuItem";
    div_Menu3.onclick = evtMenu3;
    div_Menu3.onmousemove = evtMenuOnmouseMove;
    div_Menu3.onmouseout = evtOnMouseOut;
    div_Menu3.innerHTML = "预计到达时间：" + document.all("hdUseTime").value + "分";

    //var Hr1 = document.createElement("Hr");


    div_Menu.appendChild(div_Menu1);
    div_Menu.appendChild(div_Menu3);
    //div_Menu.appendChild(Hr1);


    document.body.appendChild(div_Menu);
}


function ShowRightMenu() {
    if (document.all("myMap") != null) {//只有在显示路径图的时候，才显示此内容菜单
        if ($("div_RightMenu") == null) {
            CreateMenu();
            document.oncontextmenu = ShowMenu
            document.body.onclick = HideMenu
        } else {
            document.oncontextmenu = ShowMenu
            document.body.onclick = HideMenu
        }
    }
}

// 判断客户端浏览器
function IsIE() {
    if (navigator.appName == "Microsoft Internet Explorer") {
        return true;
    } else {
        return false;
    }
}

function ShowMenu() {
    if (event.srcElement.id != "myMap") {
        return;
    } //要求必须是在地铁路线图上点击右键才会出现此菜单；
//    document.body.onclick = HideMenu;
    var menu = $("div_RightMenu");
    menu.style.left = event.clientX + "px";
    menu.style.top = event.clientY + "px";
    menu.style.display = "block";
    return false;
}


function HideMenu() {
    if (IsIE()) $("div_RightMenu").style.display = "none";
}

function $(gID) {
    return document.getElementById(gID);
}
