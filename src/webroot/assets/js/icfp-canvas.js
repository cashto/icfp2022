class IcfpCanvas {
    constructor (elementId, paintFn, mouseEvents) {
        this.element = document.querySelector(elementId);
        this.dx = 0;
        this.dy = 0;
        this.zoom(1, 1);
        this.paintFn = paintFn;
        this.mouseEvents = mouseEvents;
        var canvas = this;
        
        var paint = function(canvas) {
            canvas.element.width = $(elementId).width();
            canvas.element.height = $(elementId).height();
            canvas.context = canvas.element.getContext('2d');
            canvas.context.fillStyle = $(canvas.element).css('background-color');
            canvas.context.fillRect(0, 0, canvas.clientX(), canvas.clientY());
            canvas.paintFn(canvas);
        }

        var onwheel = function(event, canvas) {
            var scale = event.deltaY > 0 ? 0.8 : 1.25;
            canvas.scaleX = canvas.scaleX * scale;
            canvas.scaleY = canvas.scaleY * scale;
        }   
        
        var onmousedown = function(event, canvas) {
            if ((event.buttons & 1) == 1) {
                canvas.mouseEvents.down(canvas.InvX(event.offsetX), canvas.InvY(event.offsetY));
            }            
        }

        var onmouseup = function(event, canvas) {
            if ((event.buttons & 1) == 1) {
                canvas.mouseEvents.up(canvas.InvX(event.offsetX), canvas.InvY(event.offsetY));
            }            

            canvas.lastMouseMoveEvent = null;
        }
        
        var oncontextmenu = function(event, canvas) {
            if (canvas.preventContextMenu) {
                event.preventDefault();
            }
            canvas.preventContextMenu = false;
        }
                
        var onmousemove = function(event, canvas) {
            if ((event.buttons & 1) == 1) {
                canvas.mouseEvents.move(canvas.InvX(event.offsetX), canvas.InvY(event.offsetY));
            }            

            if ((event.buttons & 2) == 0) {
                return;
            }
            
            if (canvas.lastMouseMoveEvent != null) {
                canvas.dx += (event.pageX - canvas.lastMouseMoveEvent.pageX) / canvas.scaleX;
                canvas.dy += (event.pageY - canvas.lastMouseMoveEvent.pageY) / canvas.scaleY;
                canvas.preventContextMenu = true;
            }
            
            canvas.lastMouseMoveEvent = event;
        }
        
        $(this.element).on('wheel', function(event) { onwheel(event.originalEvent, canvas); });
        $(this.element).on('contextmenu', function(event) { oncontextmenu(event, canvas); });
        $(this.element).mousedown(function(event) { onmousedown(event, canvas); });
        $(this.element).mousemove(function(event) { onmousemove(event, canvas); });
        $(this.element).mouseup(function(event) { onmouseup(event, canvas); });
        $(this.element).mouseleave(function(event) { onmouseup(event, canvas); });
        
        setInterval(function() { paint(canvas); }, 20);
    }   
    
    clientX() {
        return this.element.clientWidth;
    }

    clientY() {
        return this.element.clientHeight;
    }
    
    X(x) {
        return Math.round((x + this.dx) * this.scaleX + this.clientX() / 2) + 0.5;
    }
    
    Y(y) {
        return Math.round((y + this.dy) * this.scaleY + this.clientY() / 2) + 0.5;
    }

    InvX(x) {
        return (x - this.clientX() / 2) / this.scaleX - this.dx;
    }
    
    InvY(y) {
        return (y - this.clientY() / 2) / this.scaleY - this.dy;
    }
    
    zoom(dx, dy) {
        var x = 0.5 * this.clientX() / dx;
        var y = 0.5 * this.clientY() / dy;
        var scale = x < y ? x : y;
        this.scaleX = scale;
        this.scaleY = scale;
    }

    text(s, x, y, color) {
        this.context.font = '12pt Helvetica';
        this.context.fillStyle = color || 'black';
        this.context.fillText(s, this.X(x), this.Y(y));
    }
    
    line(x1, y1, x2, y2, color) {
        this.context.fillStyle = color || 'black';
        this.context.beginPath();
        this.context.moveTo(this.X(x1), this.Y(y1));
        this.context.lineTo(this.X(x2), this.Y(y2));
        this.context.stroke();
    }    
    
    circle(x, y, r, color) {
        this.context.fillStyle = color || 'black';
        this.context.beginPath();
        this.context.ellipse(this.X(x), this.Y(y), r * this.scaleX, r * this.scaleY, 0, 0, 2 * Math.PI);
        this.context.fill();
    }
    
    rect(x, y, dx, dy, color) {
        this.context.fillStyle = color || 'black';
        this.context.fillRect(this.X(x), this.Y(y), dx * this.scaleX, dy * this.scaleY);
    }
    
    outlinerect(x, y, dx, dy, color) {
        this.context.strokeStyle = color || 'black';
        this.context.strokeRect(this.X(x), this.Y(y), dx * this.scaleX, dy * this.scaleY);
    }    
    
    image(x, y, img, opacity) {
        this.context.globalAlpha = opacity;
        this.context.imageSmoothingEnabled = false;
        this.context.drawImage(img, this.X(x), this.Y(y), img.width * this.scaleX, img.height * this.scaleY);
    }
}