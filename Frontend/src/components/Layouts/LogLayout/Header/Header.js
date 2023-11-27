import classNames from "classnames/bind";
import images from "../../../../assets/images";
import MenuSlide from "../../../MenuSlide";
import SearchBar from "../../../SearchBar";
import CartButton from "../../../CartButton";
import { Button } from "antd";
import { UserOutlined } from "@ant-design/icons";

const cx = classNames.bind();

console.log(images.logo);
function Header() {
  return (
    <header className={cx("wrapper")}>
      <div
        className={cx("inner")}
        style={{ display: "flex", marginBottom: 10, flexDirection: "row" }}
      >
        <div className={cx("logo")}>
          <img src={images.logo} alt="1015 BookStore" />
        </div>
      </div>
    </header>
  );
}

export default Header;
