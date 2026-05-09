import L from 'leaflet';
import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';

let _MaplibreGL: any = null;

function getMaplibreGL() {
  if (_MaplibreGL) return _MaplibreGL;

  const Leaflet = (L as any).default ?? L;
  const LayerClass = Leaflet.Layer;
  if (!LayerClass || typeof LayerClass.extend !== 'function') {
    throw new Error('Leaflet.Layer.extend is not available — cannot create MaplibreGL layer');
  }

  function roundPoint(p: { x: number; y: number }) {
    return { x: Math.round(p.x), y: Math.round(p.y) };
  }

  _MaplibreGL = LayerClass.extend({
    options: {
      updateInterval: 32,
      padding: 0.1,
      interactive: false,
      pane: 'tilePane',
    },

    initialize(this: any, options: any) {
      (L as any).setOptions(this, options);
      this._throttledUpdate = (L as any).Util.throttle(this._update, this.options.updateInterval, this);
    },

    onAdd(this: any, map: any) {
      if (!this._container) {
        this._initContainer();
      }
      const paneName = this.getPaneName();
      map.getPane(paneName).appendChild(this._container);
      this._initGL();
      this._offset = this._map.containerPointToLayerPoint([0, 0]);
      if (map.options.zoomAnimation) {
        (L as any).DomEvent.on(map._proxy, (L as any).DomUtil.TRANSITION_END, this._transitionEnd, this);
      }
    },

    onRemove(this: any, map: any) {
      if (this._map._proxy && this._map.options.zoomAnimation) {
        (L as any).DomEvent.off(this._map._proxy, (L as any).DomUtil.TRANSITION_END, this._transitionEnd, this);
      }
      const paneName = this.getPaneName();
      map.getPane(paneName).removeChild(this._container);
      this._glMap.remove();
      this._glMap = null;
    },

    getEvents(this: any) {
      return {
        move: this._throttledUpdate,
        zoomanim: this._animateZoom,
        zoom: this._pinchZoom,
        zoomstart: this._zoomStart,
        zoomend: this._zoomEnd,
        resize: this._resize,
      };
    },

    getAttribution(this: any) {
      if (this.options.attributionControl) {
        return this.options.attributionControl.customAttribution;
      }
      const map = this._glMap;
      if (map && this.options.attributionControl !== false) {
        const style = map.getStyle();
        if (style && style.sources) {
          return Object.keys(style.sources)
            .map((sourceId: string) => {
              const source = map.getSource(sourceId);
              return source && typeof source.attribution === 'string' ? source.attribution.trim() : null;
            })
            .filter(Boolean)
            .join(', ');
        }
      }
      return '';
    },

    getMaplibreMap(this: any) { return this._glMap; },
    getCanvas(this: any) { return this._glMap.getCanvas(); },
    hasLoadFailed(this: any) { return this._loadFailed; },
    getSize(this: any) { return this._map.getSize().multiplyBy(1 + this.options.padding * 2); },
    getBounds(this: any) {
      const halfSize = this.getSize().multiplyBy(0.5);
      const center = this._map.latLngToContainerPoint(this._map.getCenter());
      return (L as any).latLngBounds(
        this._map.containerPointToLatLng(center.subtract(halfSize)),
        this._map.containerPointToLatLng(center.add(halfSize)),
      );
    },
    getContainer(this: any) { return this._container; },
    getPaneName(this: any) {
      return this._map.getPane(this.options.pane) ? this.options.pane : 'tilePane';
    },

    _initContainer(this: any) {
      const container = (this._container = (L as any).DomUtil.create('div', 'leaflet-gl-layer'));
      this._resizeContainer();
      const offset = this._map.getSize().multiplyBy(this.options.padding);
      const topLeft = this._map.containerPointToLayerPoint([0, 0]).subtract(offset);
      (L as any).DomUtil.setPosition(container, roundPoint(topLeft));
    },

    _resizeContainer(this: any) {
      const size = this.getSize();
      this._container.style.width = size.x + 'px';
      this._container.style.height = size.y + 'px';
    },

    _initGL(this: any) {
      const center = this._map.getCenter();
      const options = (L as any).extend({}, this.options, {
        container: this._container,
        center: [center.lng, center.lat],
        zoom: this._map.getZoom() - 1,
        attributionControl: false,
      });
      this._glMap = new maplibregl.Map(options);
      this._loadFailed = false;

      const _map = this._map;
      const _currentAttribution = this.getAttribution();
      const _getAttribution = this.getAttribution.bind(this);

      this._glMap.on('error', (e: any) => {
        console.warn('[MapLibre GL] Error:', e.error?.message || e.message || 'unknown');
        this._loadFailed = true;
      });

      this._glMap.on('load', () => {
        this._loadFailed = false;
        if (_map && _map.attributionControl) {
          _map.attributionControl.removeAttribution(_currentAttribution);
          _map.attributionControl.addAttribution(_getAttribution());
        }
      });

      const transformProto = Object.getPrototypeOf(this._glMap.transform);
      const latRangeDescriptor = Object.getOwnPropertyDescriptor(transformProto, 'latRange');
      if (!latRangeDescriptor || latRangeDescriptor.set || latRangeDescriptor.writable) {
        this._glMap.transform.latRange = null;
      }
      const maxValidLatitudeDescriptor = Object.getOwnPropertyDescriptor(transformProto, 'maxValidLatitude');
      if (!maxValidLatitudeDescriptor || maxValidLatitudeDescriptor.set || maxValidLatitudeDescriptor.writable) {
        this._glMap.transform.maxValidLatitude = Infinity;
      }
      if (this._glMap.transform._helper && this._glMap.transform._helper._latRange) {
        this._glMap.transform._helper._latRange = [-Infinity, Infinity];
      }

      this._transformGL(this._glMap);

      const canvas = this._glMap.getCanvas();
      (L as any).DomUtil.addClass(canvas, 'leaflet-image-layer');
      (L as any).DomUtil.addClass(canvas, 'leaflet-zoom-animated');
      if (this.options.interactive) {
        (L as any).DomUtil.addClass(canvas, 'leaflet-interactive');
      }
      if (this.options.className) {
        (L as any).DomUtil.addClass(canvas, this.options.className);
      }
    },

    _update(this: any) {
      if (!this._map || this._loadFailed) return;
      this._offset = this._map.containerPointToLayerPoint([0, 0]);
      if (this._zooming) return;
      const container = this._container;
      const offset = this._map.getSize().multiplyBy(this.options.padding);
      const topLeft = this._map.containerPointToLayerPoint([0, 0]).subtract(offset);
      (L as any).DomUtil.setPosition(container, roundPoint(topLeft));
      this._transformGL(this._glMap);
    },

    _transformGL(this: any, gl: any) {
      const center = this._map.getCenter();
      const tr = gl._getTransformForUpdate();
      if (tr.setCenter) {
        tr.setCenter(maplibregl.LngLat.convert([center.lng, center.lat]));
        tr.setZoom(this._map.getZoom() - 1);
        gl.transform.apply(tr);
      } else {
        tr.center = maplibregl.LngLat.convert([center.lng, center.lat]);
        tr.zoom = this._map.getZoom() - 1;
      }
      gl._fireMoveEvents();
    },

    _pinchZoom(this: any) {
      this._glMap.jumpTo({
        zoom: this._map.getZoom() - 1,
        center: this._map.getCenter(),
      });
    },

    _animateZoom(this: any, e: any) {
      if (this._loadFailed || !this._glMap?._actualCanvas) return;
      const scale = this._map.getZoomScale(e.zoom);
      const padding = this._map.getSize().multiplyBy(this.options.padding * scale);
      const viewHalf = this.getSize()._divideBy(2);
      const topLeft = this._map
        .project(e.center, e.zoom)
        ._subtract(viewHalf)
        ._add(this._map._getMapPanePos().add(padding))
        ._round();
      const offset = this._map.project(this._map.getBounds().getNorthWest(), e.zoom)._subtract(topLeft);
      (L as any).DomUtil.setTransform(this._glMap._actualCanvas, offset.subtract(this._offset), scale);
    },

    _zoomStart() { (this as any)._zooming = true; },

    _zoomEnd(this: any) {
      if (this._loadFailed || !this._glMap?._actualCanvas) {
        this._zooming = false;
        return;
      }
      const scale = this._map.getZoomScale(this._map.getZoom());
      (L as any).DomUtil.setTransform(this._glMap._actualCanvas, null, scale);
      this._zooming = false;
      this._update();
    },

    _transitionEnd(this: any) {
      if (this._loadFailed || !this._glMap?._actualCanvas) return;
      (L as any).Util.requestAnimFrame(() => {
        if (this._loadFailed || !this._glMap?._actualCanvas) return;
        const zoom = this._map.getZoom();
        const center = this._map.getCenter();
        const offset = this._map.latLngToContainerPoint(this._map.getBounds().getNorthWest());
        this._resizeContainer();
        (L as any).DomUtil.setTransform(this._glMap._actualCanvas, offset, 1);
        this._glMap.once('moveend', () => {
          this._zoomEnd();
        });
        this._glMap.jumpTo({ center, zoom: zoom - 1 });
      }, this);
    },

    _resize(this: any, e: any) { this._transitionEnd(e); },
  });

  return _MaplibreGL;
}

export function createMaplibreGLLayer(options: any) {
  const GLClass = getMaplibreGL();
  return new GLClass(options);
}
