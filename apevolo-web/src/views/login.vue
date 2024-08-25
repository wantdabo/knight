<template>
  <div class="login" :style="'background-image:url('+ Background +');'">
    <el-form
      ref="loginForm"
      :model="loginForm"
      :rules="loginRules"
      label-position="left"
      label-width="0px"
      class="login-form animated bounceIn"
    >
      <img :src="require('@/assets/images/logo.png')" alt="Knight Logo" class="custom-image">
      <br>
      <br>
      <h4 class="title">
        Welcome to Knight. 👏
      </h4>
      <el-form-item prop="username">
        <el-input v-model="loginForm.username" type="text" auto-complete="off" placeholder="账号">
          <svg-icon slot="prefix" icon-class="user" class="el-input__icon input-icon" />
        </el-input>
      </el-form-item>
      <el-form-item prop="password">
        <el-input
          v-model="loginForm.password"
          show-password
          type="password"
          auto-complete="off"
          placeholder="密码"
          @keyup.enter.native="handleLogin"
        >
          <svg-icon slot="prefix" icon-class="password" class="el-input__icon input-icon" />
        </el-input>
      </el-form-item>
      <el-form-item style="width:100%;">
        <el-button
          :loading="loading"
          size="medium"
          type="primary"
          style="width:100%;"
          @click.native.prevent="handleLogin"
        >
          <span v-if="!loading">登 录</span>
          <span v-else>登 录 中...</span>
        </el-button>
      </el-form-item>
    </el-form>
    <!--  底部  -->
    <div v-if="$store.state.settings.showFooter" class="login-copyright">
      <span v-html="$store.state.settings.footerTxt" />
    </div>
  </div>
</template>

<script>
import { encrypt } from '@/utils/rsaEncrypt'
import qs from 'qs'
import Background from '@/assets/images/background.png'

export default {
  name: 'Login',
  data() {
    return {
      Background: Background,
      codeUrl: '',
      loginForm: {
        username: 'apevolo',
        password: '123456'
      },
      loginRules: {
        username: [{ required: true, trigger: 'blur', message: '用户名不能为空' }],
        password: [{ required: true, trigger: 'blur', message: '密码不能为空' }]
      },
      loading: false,
      redirect: undefined
    }
  },
  watch: {
    $route: {
      handler: function(route) {
        const data = route.query
        if (data && data.redirect) {
          this.redirect = data.redirect
          delete data.redirect
          if (JSON.stringify(data) !== '{}') {
            this.redirect = this.redirect + '&' + qs.stringify(data, { indices: false })
          }
        }
      },
      immediate: true
    }
  },
  methods: {
    handleLogin() {
      this.$refs.loginForm.validate(valid => {
        const user = {
          username: this.loginForm.username,
          password: encrypt(this.loginForm.password)
        }
        if (valid) {
          this.loading = true
          this.$store.dispatch('Login', user).then(() => {
            this.loading = false
            this.$router.push({ path: this.redirect || '/' })
          }).catch(() => {
            this.loading = false
          })
        } else {
          console.log('error submit!!')
          return false
        }
      })
    }
  }
}
</script>

<style rel='stylesheet/scss' lang='scss'>
.login {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
  animation: animate-cloud 20s linear infinite;
}

@-webkit-keyframes animate-cloud {
  from {
    background-position: 600px 100%;
  }
  to {
    background-position: 0 100%;
  }
}

@-moz-keyframes animate-cloud {
  from {
    background-position: 600px 100%;
  }
  to {
    background-position: 0 100%;
  }
}

@-ms-keyframes animate-cloud {
  from {
    background-position: 600px 100%;
  }
  to {
    background-position: 0 100%;
  }
}

@-o-keyframes animate-cloud {
  from {
    background-position: 600px 100%;
  }
  to {
    background-position: 0 100%;
  }
}

.title {
  margin: 0 auto 30px auto;
  text-align: center;
  color: #707070;
}

.login-form {
  border-radius: 6px;
  background: #ffffff;
  width: 385px;
  padding: 25px 25px 5px 25px;
  text-align: center; /* Center align text and elements */

  .el-input {
    height: 38px;

    input {
      height: 38px;
    }
  }

  .input-icon {
    height: 39px;
    width: 14px;
    margin-left: 2px;
  }
  .custom-image {
    max-width: 100px; /* Set the maximum width */
    max-height: 100px; /* Set the maximum height */
  }
}

.login-tip {
  font-size: 13px;
  text-align: center;
  color: #bfbfbf;
}

.login-copyright {
  color: #999;
  width: 100%;
  position: fixed;
  bottom: 30px;
  text-align: center;
}

.login-code {
  width: 33%;
  display: inline-block;
  height: 38px;
  float: right;

  img {
    cursor: pointer;
    vertical-align: middle
  }
}
</style>
