
const chessCol = {
    0:'a',
    1:'b',
    2:'c',
    3:'d',
    4:'e',
    5:'f',
    6:'g',
    7:'h'
}


for(var i=0;i<8;i++)
{
 for (var j=0;j<8;j++)
 {
     const newDiv = document.createElement('div');
     newDiv.id = chessCol[j] + (8-i);
     if((i + j)%2==0)             
     newDiv.className = "light"
     else
     newDiv.className = "dark" 
     
     document.getElementById("board").appendChild(newDiv);
     
 }   
}