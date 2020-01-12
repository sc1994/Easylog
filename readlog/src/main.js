import Vue from 'vue'
import './plugins/axios'
import App from './App.vue'
import router from './router'
import store from './store'
import './plugins/element.js'
import VueInsProgressBar from 'vue-ins-progress-bar'

Vue.config.productionTip = true
const options = {
  position: 'fixed',
  show: true,
  height: '3px'
}
Vue.use(VueInsProgressBar, options)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')