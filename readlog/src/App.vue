<template>
  <div id="app">
    <div class="div-head">Header</div>
    <router-view></router-view>
  </div>
</template>

<script>
export default {
  name: "app",
  components: {}
};
</script>

<style>
body {
  margin: 0px;
}

.div-head {
  height: 5vh;
}


</style>
