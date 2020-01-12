<template>
  <div id="app">
    <div class="div-head">
      <div v-if="!edit">
        <el-link :underline="false" @click="toEdit">EsNode: {{stateNode}}</el-link>
      </div>
      <el-input placeholder="es node " v-model="node" v-else>
        <template slot="append">
          <el-button type="success" icon="el-icon-check" circle @click="submitNode"></el-button>
        </template>
      </el-input>
    </div>
    <router-view></router-view>
    <vue-ins-progress-bar></vue-ins-progress-bar>
  </div>
</template>

<script>
import { mapState, mapMutations } from "vuex";

export default {
  name: "app",
  data() {
    return {
      node: "",
      edit: false
    };
  },
  computed: {
    ...mapState({
      stateNode: state => state.es.node
    })
  },
  methods: {
    toEdit() {
      this.edit = true;
      this.node = this.stateNode;
    },
    submitNode() {
      this.setNode(this.node);
      this.edit = false;
    },
    setNode(node) {
      this.$store.commit("SER_ES_NODE", node);
      this.$axios.defaults.baseURL = this.stateNode;
      localStorage.setItem("SER_ES_NODE", this.stateNode);
    }
  },
  mounted() {
    this.$insProgress.finish();
  },
  created() { 
    let node = localStorage.getItem("SER_ES_NODE") || this.stateNode;
    this.setNode(node);
    this.$axios.defaults.baseURL = this.stateNode;
    this.$insProgress.start();

    this.$router.beforeEach((to, from, next) => {
      this.$insProgress.start();
      next();
    });

    this.$router.afterEach((to, from) => {
      this.$insProgress.finish();
    });
  }
};
</script>

<style>
body {
  margin: 0px;
}

.div-head {
  height: 6vh;
}

/* 滚动槽 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
::-webkit-scrollbar-track {
  border-radius: 3px;
  background: #909399;
  -webkit-box-shadow: inset 0 0 5px rgba(0, 0, 0, 0.08);
}
/* 滚动条滑块 */
::-webkit-scrollbar-thumb {
  border-radius: 3px;
  background: white;
  -webkit-box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.2);
}
</style>
