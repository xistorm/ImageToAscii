package handler

import (
	"github.com/gin-gonic/gin"
	"github.com/xistorm/ascii_image/pkg/service"
)

type Handler struct {
	services *service.Service
}

func NewHandler(services *service.Service) *Handler {
	return &Handler{
		services: services,
	}
}

func (h *Handler) Routes() *gin.Engine {
	router := gin.New()

	router.GET("/ping", h.GetPingHandler)

	api := router.Group("/api")
	{
		users := api.Group("/users")
		{
			users.GET("/", h.UsersHandler)
			users.PUT("/", h.CreateUserHandler)
			users.GET("/:login", h.UserHandler)
			users.DELETE("/:login", h.DeleteUserHandler)
			users.POST("/:login", h.UpdateUserHandler)
		}
	}

	return router
}
