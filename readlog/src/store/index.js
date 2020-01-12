import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    es: {
      node: "http://localhost:9222/"
    }
  },
  mutations: {
    SER_ES_NODE: (state, node) => {
      state.es.node = node;
    }
  },
  actions: {},
  modules: {}
})