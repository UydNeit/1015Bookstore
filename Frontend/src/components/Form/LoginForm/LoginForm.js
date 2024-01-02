import { Typography, ConfigProvider, Form, Input, Button, message } from "antd";
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

const Login = () => {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleLogIn = async () => {
    try {
      setLoading(true);
      setError(null);

      const values = await form.validateFields();
      const { username, password } = values;

      const requestBody = {
        sUser_username: username,
        sUser_password: password,
        rememberme: true,
      };

      const response = await fetch(
        "https://localhost:7139/api/User/authenticate",
        {
          method: "POST",
          mode: "cors",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      if (!response.ok) {
        const error = await response.text();
        if (error) {
          message.error(`${error}`);
        }
      } else {
        const responseData = await response.json();
        console.log(responseData);

        document.cookie = `accessToken=${responseData.sUser_tokenL}; path=/`;
        document.cookie = `userid=${responseData.gUser_id}; path=/`;
        message.success("Đăng nhập thành công!");
        navigate(`/`);
        window.location.reload();
      }
    } catch (error) {
      setError("Login failed. Please try again.");
      console.error("Login failed", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="background">
      <Form
        form={form}
        layout="vertical"
        className="login_form"
        initialValues={{
          remember: true,
        }}
        autoComplete="off"
      >
        <Typography style={{ fontWeight: "bolder", fontSize: 36 }}>
          Đăng nhập
        </Typography>

        <Form.Item
          className="no_margin"
          label={<p className="label">Tên đăng nhập</p>}
          name="username"
          rules={[{ required: true, message: "Please input your username!" }]}
        >
          <Input size="large" placeholder="Tên đăng nhập" />
        </Form.Item>

        <Form.Item
          className="no_margin"
          label={<p className="label">Mật khẩu</p>}
          name="password"
          rules={[{ required: true, message: "Please input your password!" }]}
        >
          <Input.Password size="large" placeholder="Mật khẩu" />
        </Form.Item>

        <Form.Item className="no_margin">
          <ConfigProvider
            theme={{
              token: {
                colorBorder: "none",
              },
            }}
          >
            <Button
              className="button"
              block
              size="large"
              type="default"
              htmlType="submit"
              onClick={handleLogIn}
            >
              Đăng nhập
            </Button>
          </ConfigProvider>
        </Form.Item>

        <div className="functions">
          <Form.Item className="no_margin">
            <a
              className="login-form-forgot"
              href="http://localhost:3000/forgot_password"
              style={{ color: "#1f9d60", fontSize: "12px" }}
            >
              Quên mật khẩu
            </a>
          </Form.Item>

          <Form.Item className="no_margin">
            <span style={{ color: "#1f9d60", fontSize: "12px" }}>
              Bạn chưa có tài khoản?{" "}
            </span>
            <a
              className="logup-form"
              href="http://localhost:3000/sign_up"
              style={{ color: "#CF4330", fontSize: "12px" }}
            >
              Đăng ký tại đây
            </a>
          </Form.Item>
        </div>
      </Form>
    </div>
  );
};

export default Login;
