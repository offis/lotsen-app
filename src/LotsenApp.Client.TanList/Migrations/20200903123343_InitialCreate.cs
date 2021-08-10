// Copyright (c) 2021 OFFIS e.V.. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//    
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//    
// 3. Neither the name of the copyright holder nor the names of its contributors
//    may be used to endorse or promote products derived from this software without
//    specific prior written permission.
//    
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.EntityFrameworkCore.Migrations;

namespace LotsenApp.Client.TanList.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tan_lists",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Tan1 = table.Column<string>(nullable: true),
                    Tan1Used = table.Column<bool>(nullable: false),
                    Tan2 = table.Column<string>(nullable: true),
                    Tan2Used = table.Column<bool>(nullable: false),
                    Tan3 = table.Column<string>(nullable: true),
                    Tan3Used = table.Column<bool>(nullable: false),
                    Tan4 = table.Column<string>(nullable: true),
                    Tan4Used = table.Column<bool>(nullable: false),
                    Tan5 = table.Column<string>(nullable: true),
                    Tan5Used = table.Column<bool>(nullable: false),
                    Tan6 = table.Column<string>(nullable: true),
                    Tan6Used = table.Column<bool>(nullable: false),
                    Tan7 = table.Column<string>(nullable: true),
                    Tan7Used = table.Column<bool>(nullable: false),
                    Tan8 = table.Column<string>(nullable: true),
                    Tan8Used = table.Column<bool>(nullable: false),
                    Tan9 = table.Column<string>(nullable: true),
                    Tan9Used = table.Column<bool>(nullable: false),
                    Tan10 = table.Column<string>(nullable: true),
                    Tan10Used = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tan_lists", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tan_lists");
        }
    }
}
