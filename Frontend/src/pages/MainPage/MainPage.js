import React, { useState } from "react";
import { Button, Card, Col, Row } from "antd";
import MenuSlide from "../../components/MenuSlide";
import { useNavigate } from "react-router-dom";
import { items } from "../../components/Data";
const { Meta } = Card;

function MainPage() {


  const navigate = useNavigate();
  const [selectedMenu, setSelectedMenu] = useState(null);

  const handleMenuSelect = (selectedValue) => {
    setSelectedMenu(selectedValue);
    navigate(`/${selectedValue}`);
  };


  const handleProductClick = (item) => {
    // Chuyển hướng đến trang chi tiết sản phẩm và truyền thông tin sản phẩm qua state
    navigate(`/product-detail/${item.id}`, { state: { item } });
  };

  const filteredItems = selectedMenu
    ? items.filter((item) => item.category === selectedMenu)
    : items;

  return (
    <div>
      <MenuSlide onMenuSelect={handleMenuSelect} />
      {/* Display filtered data */}
      {filteredItems.map((item) => (
        <Row gutter={16} key={item.title}>
          <Col span={8}>
            <Card
              hoverable
              style={{
                width: 240,
              }}
              cover={<img alt={item.title} src={item.img} />}
              onClick={() => handleProductClick(item)}
            >
              <Meta title={item.title} />
              <div>Price: {item.price}</div>
              <Button>Add to cart</Button>
            </Card>
          </Col>
        </Row>
      ))}
    </div>
  );
}

export default MainPage;
